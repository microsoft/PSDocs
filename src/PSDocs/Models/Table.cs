using System.Collections.Generic;

namespace PSDocs.Models
{
    public sealed class Table : DocumentNode
    {
        public Table()
        {
            Header = new List<string>();
            Rows = new List<string[]>();
        }

        public override DocumentNodeType Type => DocumentNodeType.Table;

        public List<string[]> Rows { get; set; }

        public List<string> Header { get; set; }
    }
}
