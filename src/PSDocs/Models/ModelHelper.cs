namespace PSDocs.Models
{
    public static class ModelHelper
    {
        public static Document NewDocument()
        {
            return new Document
            {

            };
        }

        public static Section NewSection(string name, int level)
        {
            return new Section
            {
                Title = name,
                Level = level
            };
        }

        public static TableBuilder Table()
        {
            return new TableBuilder();
        }

        public static Code NewCode()
        {
            return new Code
            {

            };
        }

        public static BlockQuote BlockQuote(string info, string title)
        {
            return new BlockQuote
            {
                Info = info,
                Title = title
            };
        }

        public static Text Text(string value)
        {
            return new Text
            {
                Content = value
            };
        }
    }
}
