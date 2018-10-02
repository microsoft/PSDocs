namespace PSDocs.Models
{
    public sealed class BlockQuote : DocumentNode
    {
        public override DocumentNodeType Type => DocumentNodeType.BlockQuote;

        public string Info { get; set; }

        public string[] Content { get; set; }
    }
}