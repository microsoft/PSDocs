// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSDocs.Models
{
    public sealed class Section : SectionNode
    {
        public override DocumentNodeType Type
        {
            get { return DocumentNodeType.Section; }
        }
    }
}
