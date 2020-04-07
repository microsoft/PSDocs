using System.Collections.Specialized;

namespace PSDocs.Models
{
    public sealed class Document : SectionNode
    {
        public readonly string Name;

        public Document(string name)
        {
            Name = name;
            Metadata = new OrderedDictionary();
        }

        public override DocumentNodeType Type
        {
            get { return DocumentNodeType.Document; }
        }

        public OrderedDictionary Metadata { get; set; }
    }
}
