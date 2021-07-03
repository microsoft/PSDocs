// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PSDocs
{
    internal static class HashtableExtensions
    {
        [DebuggerStepThrough]
        public static void AddUnique(this Hashtable hashtable, Hashtable values)
        {
            if (values == null)
                return;

            foreach (var key in values.Keys)
                if (!hashtable.ContainsKey(key))
                    hashtable.Add(key, values[key]);
        }

        /// <summary>
        /// Build index to allow mapping.
        /// </summary>
        public static Dictionary<string, object> BuildIndex(this Hashtable hashtable, bool caseSensitive = false)
        {
            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            var index = new Dictionary<string, object>(comparer);
            foreach (DictionaryEntry entry in hashtable)
                index.Add(entry.Key.ToString(), entry.Value);

            return index;
        }
    }
}
