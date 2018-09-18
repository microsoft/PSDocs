using System;
using System.Collections;
using System.Collections.Generic;

namespace PSDocs.Models
{
    public sealed class TableBuilder
    {
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

            object value;

            if (index.TryGetValue("name", out value))
            {
                header.Label = (string)value;
            }

            if (index.TryGetValue("label", out value))
            {
                header.Label = (string)value;
            }

            if (index.TryGetValue("width", out value))
            {
                header.Width = (int)value;
            }

            if (index.TryGetValue("alignment", out value))
            {
                header.Alignment = (Alignment)Enum.Parse(typeof(Alignment), (string)value, true);
            }

            // Validate header

            if (string.IsNullOrEmpty(header.Label))
            {
                throw new Exception("Label must be set");
            }

            _Headers.Add(header);
        }

        public IDictionary<string, object> GetIndex(Hashtable hashtable)
        {
            // Build index to allow mapping
            var index = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (DictionaryEntry entry in hashtable)
            {
                index.Add(entry.Key.ToString(), entry.Value);
            }

            return index;
        }

        public IDictionary<string, object> GetPropertyFilter(Hashtable hashtable)
        {
            var index = GetIndex(hashtable);

            if (index.ContainsKey("alignment"))
            {
                index.Remove("alignment");
            }

            if (index.ContainsKey("width"))
            {
                index.Remove("width");
            }

            if (index.ContainsKey("name"))
            {
                index["label"] = index["name"];
                index.Remove("name");
            }

            return index;
        }
    }
}
