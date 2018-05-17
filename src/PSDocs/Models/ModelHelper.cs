namespace PSDocs.Models
{
    public static class ModelHelper
    {
        public static Section NewSection(string name, int level)
        {
            return new Section
            {
                Content = name,
                Level = level
            };
        }

        public static Table NewTable()
        {
            return new Table
            {

            };
        }
    }
}
