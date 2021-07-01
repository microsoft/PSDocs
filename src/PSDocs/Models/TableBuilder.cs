// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Pipeline;
using PSDocs.Resources;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PSDocs.Models
{
    public sealed class TableBuilder
    {
        private const string FIELD_NAME = "name";
        private const string FIELD_LABEL = "label";
        private const string FIELD_WIDTH = "width";
        private const string FIELD_ALIGNMENT = "alignment";

        private readonly List<TableColumnHeader> _Headers;

        public TableBuilder()
        {
            _Headers = new List<TableColumnHeader>();
        }

        public Table Build()
        {
            return new Table
            {
                Headers = _Headers
            };
        }

        public void Header(string label)
        {
            _Headers.Add(new TableColumnHeader
            {
                Label = label
            });
        }

        public void Header(Hashtable hashtable)
        {
            var header = new TableColumnHeader();

            // Build index to allow mapping
            var index = GetIndex(hashtable);

            // Start loading matching values
            if (index.TryGetValue(FIELD_NAME, out object value))
                header.Label = (string)value;

            if (index.TryGetValue(FIELD_LABEL, out value))
                header.Label = (string)value;

            if (index.TryGetValue(FIELD_WIDTH, out value))
                header.Width = (int)value;

            if (index.TryGetValue(FIELD_ALIGNMENT, out value))
                header.Alignment = (Alignment)Enum.Parse(typeof(Alignment), (string)value, true);

            // Validate header
            if (string.IsNullOrEmpty(header.Label))
                throw new RuntimeException(PSDocsResources.LabelNullOrEmpty);

            _Headers.Add(header);
        }

        public IDictionary<string, object> GetIndex(Hashtable hashtable)
        {
            // Build index to allow mapping
            var index = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in hashtable)
                index.Add(entry.Key.ToString(), entry.Value);

            return index;
        }

        public IDictionary<string, object> GetPropertyFilter(Hashtable hashtable)
        {
            var index = GetIndex(hashtable);
            if (index.ContainsKey(FIELD_ALIGNMENT))
                index.Remove(FIELD_ALIGNMENT);

            if (index.ContainsKey(FIELD_WIDTH))
                index.Remove(FIELD_WIDTH);

            if (index.ContainsKey(FIELD_NAME))
            {
                index[FIELD_LABEL] = index[FIELD_NAME];
                index.Remove(FIELD_NAME);
            }
            return index;
        }
    }
}
