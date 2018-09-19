using System.ComponentModel;

namespace PSDocs.Configuration
{
    public sealed class MarkdownOption
    {
        private const string DEFAULT_WRAP_SEPARATOR = " ";
        private const MarkdownEncoding DEFAULT_ENCODING = MarkdownEncoding.Default;
        private const bool DEFAULT_SKIP_EMPTY_SECTIONS = true;
        private const ColumnPadding DEFAULT_COLUMN_PADDING = ColumnPadding.MatchHeader;
        private const EdgePipeOption DEFAULT_USE_EDGE_PIPES = EdgePipeOption.WhenRequired;

        public MarkdownOption()
        {
            WrapSeparator = DEFAULT_WRAP_SEPARATOR;
            Encoding = DEFAULT_ENCODING;
            SkipEmptySections = DEFAULT_SKIP_EMPTY_SECTIONS;
            ColumnPadding = DEFAULT_COLUMN_PADDING;
            UseEdgePipes = DEFAULT_USE_EDGE_PIPES;
        }

        [DefaultValue(DEFAULT_WRAP_SEPARATOR)]
        public string WrapSeparator { get; set; }

        [DefaultValue(DEFAULT_ENCODING)]
        public MarkdownEncoding Encoding { get; set; }

        [DefaultValue(DEFAULT_SKIP_EMPTY_SECTIONS)]
        public bool SkipEmptySections { get; set; }

        /// <summary>
        /// Determines how table columns are padded.
        /// </summary>
        [DefaultValue(DEFAULT_COLUMN_PADDING)]
        public ColumnPadding ColumnPadding { get; set; }

        /// <summary>
        /// Are pipes used for the edge of tables? Single column table always require edge pipes.
        /// </summary>
        [DefaultValue(DEFAULT_USE_EDGE_PIPES)]
        public EdgePipeOption UseEdgePipes { get; set; }
    }
}
