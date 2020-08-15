using System.Collections.Specialized;

namespace PSDocs.Models
{
    public sealed class Document : SectionNode
    {
        public readonly string Name;
        public readonly string Culture;

        public Document(string name, string culture)
        {
            Name = name;
            Culture = culture;
            Metadata = new OrderedDictionary();
        }

        public override DocumentNodeType Type
        {
            get { return DocumentNodeType.Document; }
        }

        public OrderedDictionary Metadata { get; set; }
    }
}
