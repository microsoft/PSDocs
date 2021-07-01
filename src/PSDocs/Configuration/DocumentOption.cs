// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSDocs.Configuration
{
    public sealed class DocumentOption
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

        public string[] Include { get; set; }

        public string[] Tag { get; set; }
    }
}
