// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace PSDocs.Runtime
{
    /// <summary>
    /// A set of custom configuration values that are exposed at runtime.
    /// </summary>
    public sealed class Configuration : DynamicObject
    {
        private readonly RunspaceContext _Context;

        internal Configuration(RunspaceContext context)
        {
            _Context = context;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (_Context == null || binder == null || string.IsNullOrEmpty(binder.Name))
                return false;

            // Get from configuration
            return TryGetValue(binder.Name, out result);
        }

        public string[] GetStringValues(string configurationKey)
        {
            if (!TryGetValue(configurationKey, out var value) || value == null)
                return System.Array.Empty<string>();

            if (value is string valueT)
                return new string[] { valueT };

            if (value is string[] result)
                return result;

            if (value is IEnumerable c)
            {
                var cList = new List<string>();
                foreach (var v in c)
                    cList.Add(v.ToString());

                return cList.ToArray();
            }
            return new string[] { value.ToString() };
        }

        public object GetValueOrDefault(string configurationKey, object defaultValue)
        {
            if (!TryGetValue(configurationKey, out var value) || value == null)
                return defaultValue;

            return value;
        }

        public bool GetBoolOrDefault(string configurationKey, bool defaultValue)
        {
            if (!TryGetValue(configurationKey, out var value) || !TryBool(value, out var bvalue))
                return defaultValue;

            return bvalue;
        }

        private bool TryGetValue(string name, out object value)
        {
            value = null;
            if (_Context == null)
                return false;

            return _Context.Pipeline.Option.Configuration.TryGetValue(name, out value);
        }

        private bool TryBool(object o, out bool value)
        {
            value = default;
            if (o is bool bvalue || (o is string svalue && bool.TryParse(svalue, out bvalue)))
            {
                value = bvalue;
                return true;
            }
            return false;
        }
    }
}
