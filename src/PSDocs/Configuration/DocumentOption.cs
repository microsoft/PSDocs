namespace PSDocs.Configuration
{
    public sealed class DocumentOption
    {
        public DocumentOption()
        {
            Include = null;
            Tag = null;
        }

        internal DocumentOption(DocumentOption option)
        {
            Include = option.Include;
            Tag = option.Tag;
        }

        public string[] Include { get; set; }

        public string[] Tag { get; set; }
    }
}
