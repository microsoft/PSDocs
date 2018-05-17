namespace PSDocs.Models
{
    public sealed class Note : DocumentNode
    {
        public override DocumentNodeType Type => DocumentNodeType.Note;

        public string[] Content { get; set; }
    }
}
