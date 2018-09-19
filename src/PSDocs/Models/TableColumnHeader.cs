namespace PSDocs.Models
{
    public sealed class TableColumnHeader
    {
        public TableColumnHeader()
        {
            Alignment = Alignment.Undefined;
        }

        public Alignment Alignment { get; set; }

        public string Label { get; set; }

        public int Width { get; set; }
    }
}
