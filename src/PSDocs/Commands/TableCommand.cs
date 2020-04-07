
using PSDocs.Models;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Format, LanguageKeywords.Table)]
    internal sealed class TableCommand : KeywordCmdlet
    {
        private TableBuilder _TableBuilder;
        private List<PSObject> _RowData;
        private List<PropertyReader> _Reader;

        internal delegate string PropertyReader(PSObject value);

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Parameter(Position = 0)]
        public object[] Property { get; set; }

        protected override void BeginProcessing()
        {
            _TableBuilder = GetBuilder().Table();
            _RowData = new List<PSObject>();

            if (Property == null || Property.Length == 0)
                return;

            _Reader = new List<PropertyReader>();
            for (var i = 0; i < Property.Length; i++)
            {
                if (Property[i] is Hashtable propertyExpression)
                {
                    _TableBuilder.Header(propertyExpression);
                    if (propertyExpression["Expression"] is ScriptBlock expression)
                        _Reader.Add((PSObject value) => ReadPropertyByExpression(value, expression));
                    else
                    {
                        var propertyName = propertyExpression["Expression"].ToString();
                        _Reader.Add((PSObject value) => ReadPropertyByName(value, propertyName));
                    }
                }
                else
                {
                    var propertyName = Property[i].ToString();
                    _TableBuilder.Header(propertyName);
                    _Reader.Add((PSObject value) => ReadPropertyByName(value, propertyName));
                }
            }
        }

        protected override void ProcessRecord()
        {
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
            return value.Properties[propertyName].Value?.ToString();
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

        private string[] ReadFields(PSObject row)
        {
            var fields = new List<string>();
            for (var i = 0; i < _Reader.Count; i++)
                fields.Add(_Reader[i].Invoke(row));

            return fields.ToArray();
        }
    }
}
