using System.ComponentModel;

namespace PSDocs.Configuration
{
    public sealed class MarkdownOption
    {
        private const string DEFAULT_WRAP_SEPARATOR = " ";

        public MarkdownOption()
        {
            WrapSeparator = DEFAULT_WRAP_SEPARATOR;
        }

        [DefaultValue(DEFAULT_WRAP_SEPARATOR)]
        public string WrapSeparator { get; set; }
    }
}