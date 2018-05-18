using System.Collections.Specialized;

namespace PSDocs.Models
{
    public sealed class Document : DocumentNode
    {
        public Document()
        {
            Title = string.Empty;
            Metadata = new OrderedDictionary();
        }

        public override DocumentNodeType Type
        {
            get { return DocumentNodeType.Document; }
        }

        public string Title { get; set; }

        public OrderedDictionary Metadata { get; set; }
    }
}
