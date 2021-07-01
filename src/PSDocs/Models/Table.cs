// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace PSDocs.Models
{
    public sealed class Table : DocumentNode
    {
        public Table()
        {
            Headers = new List<TableColumnHeader>();
            Rows = new List<string[]>();
        }

        public override DocumentNodeType Type => DocumentNodeType.Table;

        public List<string[]> Rows { get; set; }

        public List<TableColumnHeader> Headers { get; set; }
    }
}
