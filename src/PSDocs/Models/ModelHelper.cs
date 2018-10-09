using PSDocs.Configuration;
using System.IO;

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

        public static Include Include(string baseDirectory, string culture, string fileName, bool useCulture)
        {
            var absolutePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(baseDirectory, (useCulture ? culture : string.Empty), fileName);

            if (!Path.IsPathRooted(absolutePath))
            {
                absolutePath = Path.Combine(PSDocumentOption.GetWorkingPath(), absolutePath);
            }

            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException("The included file was not found.", absolutePath);
            }

            return new Include
            {
                Path = absolutePath
            };
        }
    }
}
