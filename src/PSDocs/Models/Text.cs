namespace PSDocs.Models
{
    public sealed class Text : DocumentNode
    {
        public override DocumentNodeType Type => DocumentNodeType.Text;

        public string Content { get; set; }
    }
}
