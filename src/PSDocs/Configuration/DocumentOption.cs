// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace PSDocs.Configuration
{
    public sealed class DocumentOption : IEquatable<DocumentOption>
    {
        internal static readonly DocumentOption Default = new DocumentOption
        {
            Include = null,
            Tag = null,
        };

        public DocumentOption()
        {
            Include = null;
            Tag = null;
        }

        internal DocumentOption(DocumentOption option)
        {
            Include = option.Include;
            Tag = option.Tag;
        }

        public override bool Equals(object obj)
        {
            return obj is DocumentOption option && Equals(option);
        }

        public bool Equals(DocumentOption other)
        {
            return other != null &&
                Include == other.Include &&
                Tag == other.Tag;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (Include != null ? Include.GetHashCode() : 0);
                hash = hash * 23 + (Tag != null ? Tag.GetHashCode() : 0);
                return hash;
            }
        }

        internal static DocumentOption Combine(DocumentOption o1, DocumentOption o2)
        {
            return new DocumentOption(o1)
            {
                Include = o1.Include ?? o2.Include,
                Tag = o1.Tag ?? o2.Tag
            };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Exposed for serialization.")]
        public string[] Include { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Exposed for serialization.")]
        public string[] Tag { get; set; }

        internal void Load(EnvironmentHelper env)
        {
            if (env.TryStringArray("PSDOCS_DOCUMENT_INCLUDE", out string[] include))
                Include = include;

            if (env.TryStringArray("PSDOCS_DOCUMENT_TAG", out string[] tag))
                Tag = tag;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopStringArray("Document.Include", out string[] include))
                Include = include;

            if (index.TryPopStringArray("Document.Tag", out string[] tag))
                Tag = tag;
        }
    }
}
