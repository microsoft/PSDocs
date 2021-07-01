// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSDocs.Models
{
    public sealed class Code : DocumentNode
    {
        public override DocumentNodeType Type => DocumentNodeType.Code;

        public string Content { get; set; }

        public string Info { get; set; }
    }
}
