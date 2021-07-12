// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PSDocs
{
    internal static class DictionaryExtensions
    {
        [DebuggerStepThrough]
        public static bool TryPopValue(this IDictionary<string, object> dictionary, string key, out object value)
        {
            return dictionary.TryGetValue(key, out value) && dictionary.Remove(key);
        }

        [DebuggerStepThrough]
        public static bool TryPopValue<T>(this IDictionary<string, object> dictionary, string key, out T value)
        {
            value = default;
            if (TryPopValue(dictionary, key, out object v) && ObjectHelper.GetBaseObject(v) is T result)
            {
                value = result;
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        public static bool TryPopBool(this IDictionary<string, object> dictionary, string key, out bool value)
        {
            value = default;
            return TryPopValue(dictionary, key, out object v) && bool.TryParse(v.ToString(), out value);
        }

        [DebuggerStepThrough]
        public static bool TryPopString(this IDictionary<string, object> dictionary, string key, out string value)
        {
            return TryPopValue(dictionary, key, out value);
        }

        [DebuggerStepThrough]
        public static bool TryPopEnum<TEnum>(this IDictionary<string, object> dictionary, string key, out TEnum value) where TEnum : struct
        {
            value = default;
            return TryPopString(dictionary, key, out string result) && Enum.TryParse(result, out value);
        }

        [DebuggerStepThrough]
        public static bool TryPopStringArray(this IDictionary<string, object> dictionary, string key, out string[] value)
        {
            value = null;
            if (!TryPopValue(dictionary, key, out object result))
                return false;

            var o = ObjectHelper.GetBaseObject(result);
            value = o.GetType().IsArray ? ((object[])o).OfType<string>().ToArray() : new string[] { o.ToString() };
            return true;
        }

        [DebuggerStepThrough]
        public static bool TryGetBool(this IDictionary<string, object> dictionary, string key, out bool? value)
        {
            value = null;
            if (!dictionary.TryGetValue(key, out object o))
                return false;

            if (o is bool bvalue || (o is string svalue && bool.TryParse(svalue, out bvalue)))
            {
                value = bvalue;
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        public static bool TryGetLong(this IDictionary<string, object> dictionary, string key, out long? value)
        {
            value = null;
            if (!dictionary.TryGetValue(key, out object o))
                return false;

            if (o is long lvalue || (o is string svalue && long.TryParse(svalue, out lvalue)))
            {
                value = lvalue;
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        public static bool TryGetString(this IDictionary<string, object> dictionary, string key, out string value)
        {
            value = null;
            if (!dictionary.TryGetValue(key, out object o))
                return false;

            if (o is string sValue)
            {
                value = sValue;
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        public static bool TryGetStringArray(this IDictionary<string, object> dictionary, string key, out string[] value)
        {
            value = null;
            if (!dictionary.TryGetValue(key, out object o))
                return false;

            return TryStringArray(o, out value);
        }

        [DebuggerStepThrough]
        public static void AddUnique(this IDictionary<string, object> dictionary, IEnumerable<KeyValuePair<string, object>> values)
        {
            if (values == null)
                return;

            foreach (var kv in values)
            {
                if (!dictionary.ContainsKey(kv.Key))
                    dictionary.Add(kv.Key, kv.Value);
            }
        }

        [DebuggerStepThrough]
        private static bool TryStringArray(object o, out string[] value)
        {
            value = default;
            if (o == null)
                return false;

            value = o.GetType().IsArray ? ((object[])o).OfType<string>().ToArray() : new string[] { o.ToString() };
            return true;
        }
    }
}
