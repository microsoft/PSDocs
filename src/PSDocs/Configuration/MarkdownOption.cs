using System.ComponentModel;

namespace PSDocs.Configuration
{
    public sealed class MarkdownOption
    {
        private const string DEFAULT_WRAP_SEPARATOR = " ";

        private const MarkdownEncoding DEFAULT_ENCODING = MarkdownEncoding.Default;

        private const bool DEFAULT_SKIP_EMPTY_SECTIONS = true;

        public MarkdownOption()
        {
            WrapSeparator = DEFAULT_WRAP_SEPARATOR;
            Encoding = DEFAULT_ENCODING;
            SkipEmptySections = DEFAULT_SKIP_EMPTY_SECTIONS;
        }

        [DefaultValue(DEFAULT_WRAP_SEPARATOR)]
        public string WrapSeparator { get; set; }

        [DefaultValue(DEFAULT_ENCODING)]
        public MarkdownEncoding Encoding { get; set; }

        [DefaultValue(DEFAULT_SKIP_EMPTY_SECTIONS)]
        public bool SkipEmptySections { get; set; }
    }
}
