using System.Collections.Generic;

namespace PSDocs.Models
{
    public sealed class Section : DocumentNode
    {
        public Section()
        {
            Node = new List<DocumentNode>();
        }

        public override DocumentNodeType Type
        {
            get { return DocumentNodeType.Section; }
        }

        public string Content { get; set; }

        public int Level { get; set; }

        public List<DocumentNode> Node { get; set; }
    }
}
