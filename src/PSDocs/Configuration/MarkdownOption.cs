using System.ComponentModel;
using System.Text;

namespace PSDocs.Configuration
{
    public sealed class MarkdownOption
    {
        private const string DEFAULT_WRAP_SEPARATOR = " ";

        private const MarkdownEncoding DEFAULT_ENCODING = MarkdownEncoding.Default;

        public MarkdownOption()
        {
            WrapSeparator = DEFAULT_WRAP_SEPARATOR;
            Encoding = DEFAULT_ENCODING;
        }

        [DefaultValue(DEFAULT_WRAP_SEPARATOR)]
        public string WrapSeparator { get; set; }

        [DefaultValue(DEFAULT_ENCODING)]
        public MarkdownEncoding Encoding { get; set; }
    }
}