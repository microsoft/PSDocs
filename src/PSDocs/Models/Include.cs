namespace PSDocs.Models
{
    public sealed class Include : DocumentNode
    {
        public override DocumentNodeType Type => DocumentNodeType.Include;

        public string Path { get; set; }
    }
}