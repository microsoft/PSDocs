namespace PSDocs.Models
{
    public sealed class Warning : DocumentNode
    {
        public override DocumentNodeType Type => DocumentNodeType.Warning;

        public string[] Content { get; set; }
    }
}
