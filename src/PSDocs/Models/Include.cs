
namespace PSDocs.Models
{
    public sealed class Include : DocumentNode
    {
        public override DocumentNodeType Type => DocumentNodeType.Include;

        public string Path { get; set; }

        public string Content { get; set; }

        public override string ToString()
        {
            return Content;
        }
    }
}
