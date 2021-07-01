// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Format, LanguageKeywords.Table)]
    internal sealed class TableCommand : KeywordCmdlet
    {
        private const string ObjectKey_Label = "label";
        private const string ObjectKey_Name = "name";

        private TableBuilder _TableBuilder;
        private List<PSObject> _RowData;
        private PropertyReader _Reader;

        internal sealed class PropertyReader
        {
            private readonly Dictionary<string, GetProperty> _Map;
            private readonly List<string> _Properties;

            public PropertyReader()
            {
                _Map = new Dictionary<string, GetProperty>();
                _Properties = new List<string>();
            }

            public void Add(string propertyName, GetProperty get)
            {
                if (string.IsNullOrEmpty(propertyName) || _Map.ContainsKey(propertyName))
                    return;

                _Map.Add(propertyName, get);
                _Properties.Add(propertyName);
            }

            public IEnumerator<GetProperty> GetEnumerator()
            {
                for (var i = 0; i < _Properties.Count; i++)
                {
                    yield return _Map[_Properties[i]];
                }
            }
        }

        internal delegate string GetProperty(PSObject value);

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Parameter(Position = 0)]
        public object[] Property { get; set; }

        protected override void BeginProcessing()
        {
            _TableBuilder = ModelHelper.Table();
            _RowData = new List<PSObject>();
            BuildReader();
        }

        private void BuildReader()
        {
            _Reader = new PropertyReader();
            if (Property == null || Property.Length == 0)
                return;

            for (var i = 0; i < Property.Length; i++)
            {
                if (Property[i] is Hashtable propertyExpression)
                {
                    _TableBuilder.Header(propertyExpression);
                    if (propertyExpression["Expression"] is ScriptBlock expression)
                    {
                        var propertyName = GetPropertyName(propertyExpression);
                        _Reader.Add(propertyName, (PSObject value) => ReadPropertyByExpression(value, expression));
                    }
                    else
                    {
                        var propertyName = propertyExpression["Expression"].ToString();
                        _Reader.Add(propertyName, (PSObject value) => ReadPropertyByName(value, propertyName));
                    }
                }
                else
                {
                    var propertyName = Property[i].ToString();
                    _TableBuilder.Header(propertyName);
                    _Reader.Add(propertyName, (PSObject value) => ReadPropertyByName(value, propertyName));
                }
            }
        }

        protected override void ProcessRecord()
        {
            if (Property == null || Property.Length == 0)
            {
                // Extract out the header column names based on the resulting objects
                foreach (var property in InputObject.Properties)
                {
                    var propertyName = property.Name.ToString();
                    _TableBuilder.Header(propertyName);
                    _Reader.Add(propertyName, (PSObject value) => ReadPropertyByName(value, propertyName));
                }
            }
            _RowData.Add(InputObject);
        }

        protected override void EndProcessing()
        {
            var table = _TableBuilder.Build();
            var rows = new List<string[]>();

            for (var i = 0; i < _RowData.Count; i++)
                rows.Add(ReadFields(_RowData[i]));

            table.Rows = rows;
            if (table.Rows.Count > 0)
                WriteObject(table);
        }

        private static string ReadPropertyByName(PSObject value, string propertyName)
        {
            return value.Properties[propertyName]?.Value?.ToString();
        }

        private static string ReadPropertyByExpression(PSObject value, ScriptBlock expression)
        {
            var variables = new List<PSVariable>(new PSVariable[] { new PSVariable("_", value) });
            var output = GetPSObject(expression.InvokeWithContext(null, variables, null));
            return TryString(output, out string soutput) ? soutput : output.ToString();
        }

        private static PSObject GetPSObject(Collection<PSObject> collection)
        {
            if (collection == null || collection.Count == 0)
                return null;

            return collection[0];
        }

        private static string GetPropertyName(Hashtable propertyExpression)
        {
            if (propertyExpression.ContainsKey(ObjectKey_Label))
                return propertyExpression[ObjectKey_Label].ToString();

            if (propertyExpression.ContainsKey(ObjectKey_Name))
                return propertyExpression[ObjectKey_Name].ToString();

            return null;
        }

        private string[] ReadFields(PSObject row)
        {
            if (row == null)
                return Array.Empty<string>();

            var fields = new List<string>();
            foreach (var getter in _Reader)
                fields.Add(getter.Invoke(row));

            return fields.ToArray();
        }
    }
}
