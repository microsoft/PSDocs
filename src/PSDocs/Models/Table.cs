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

        public override DocumentNodeType Type
        {
            get { return DocumentNodeType.Table; }
        }

        public IEnumerable<string[]> Rows { get; set; }

        public IEnumerable<string> Header { get; set; }
    }
}
