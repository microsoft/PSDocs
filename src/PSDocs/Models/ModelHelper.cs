// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.IO;
using PSDocs.Configuration;

namespace PSDocs.Models
{
    internal static class ModelHelper
    {
        public static Document NewDocument()
        {
            return new Document(null);
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
            return Include(baseDirectory, culture, fileName, useCulture, null);
        }

        internal static Include Include(string baseDirectory, string culture, string fileName, bool useCulture, IDictionary replace)
        {
            baseDirectory = PSDocumentOption.GetRootedPath(baseDirectory);
            var absolutePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(baseDirectory, (useCulture ? culture : string.Empty), fileName);
            var result = new Include
            {
                Path = absolutePath
            };
            if (result.Exists)
            {
                var text = File.ReadAllText(absolutePath);
                if (replace != null && replace.Count > 0)
                {
                    foreach (var key in replace.Keys)
                    {
                        var k = key?.ToString();
                        var v = replace[key]?.ToString();
                        text = text.Replace(k, v);
                    }
                }
                result.Content = text;
            }
            return result;
        }
    }
}
