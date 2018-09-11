using System;
using System.Collections.Generic;

namespace PSDocs.Models
{
    public sealed class DocumentFilter
    {
        private readonly HashSet<string> _AcceptedNames;
        private readonly HashSet<string> _RequiredTags;

        private DocumentFilter(string[] name, string[] tag)
        {
            _AcceptedNames = new HashSet<string>(name, StringComparer.OrdinalIgnoreCase);
            _RequiredTags = new HashSet<string>(tag, StringComparer.OrdinalIgnoreCase);
        }

        public static DocumentFilter Create(string[] name, string[] tag)
        {
            return new DocumentFilter(
                name: name ?? new string[] { },
                tag: tag ?? new string[] { }
            );
        }

        public bool Match(string name, string[] tag)
        {
            // If name is filtered, the name must be listed
            if (_AcceptedNames.Count > 0 && !_AcceptedNames.Contains(name))
            {
                return false;
            }

            // Check if no tags are required
            if (_RequiredTags.Count == 0)
            {
                return true;
            }

            // Check for impossible match
            if (tag == null || _RequiredTags.Count > tag.Length)
            {
                return false;
            }

            // Check each tag
            foreach (var t in tag)
            {
                if (!_RequiredTags.Contains(t))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
