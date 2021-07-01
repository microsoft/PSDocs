// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSDocs.Models
{
    public sealed class BlockQuote : DocumentNode
    {
        public override DocumentNodeType Type => DocumentNodeType.BlockQuote;

        public string Info { get; set; }

        public string Title { get; set; }

        public string[] Content { get; set; }
    }
}