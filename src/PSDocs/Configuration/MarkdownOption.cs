
using System.ComponentModel;

namespace PSDocs.Configuration
{
    public sealed class MarkdownOption
    {
        private const string DEFAULT_WRAP_SEPARATOR = " ";
        private const MarkdownEncoding DEFAULT_ENCODING = MarkdownEncoding.Default;
        private const bool DEFAULT_SKIP_EMPTY_SECTIONS = true;
        private const ColumnPadding DEFAULT_COLUMN_PADDING = Configuration.ColumnPadding.MatchHeader;
        private const EdgePipeOption DEFAULT_USE_EDGE_PIPES = EdgePipeOption.WhenRequired;

        internal static readonly MarkdownOption Default = new MarkdownOption
        {
            ColumnPadding = DEFAULT_COLUMN_PADDING,
            Encoding = DEFAULT_ENCODING,
            SkipEmptySections = DEFAULT_SKIP_EMPTY_SECTIONS,
            UseEdgePipes = DEFAULT_USE_EDGE_PIPES,
            WrapSeparator = DEFAULT_WRAP_SEPARATOR,
        };

        public MarkdownOption()
        {
            ColumnPadding = null;
            Encoding = null;
            SkipEmptySections = null;
            UseEdgePipes = null;
            WrapSeparator = null;
        }

        internal MarkdownOption(MarkdownOption option)
        {
            ColumnPadding = option.ColumnPadding;
            Encoding = option.Encoding;
            SkipEmptySections = option.SkipEmptySections;
            UseEdgePipes = option.UseEdgePipes;
            WrapSeparator = option.WrapSeparator;
        }

        public override bool Equals(object obj)
        {
            return obj is MarkdownOption option && Equals(option);
        }

        public bool Equals(MarkdownOption other)
        {
            return other != null &&
                ColumnPadding == other.ColumnPadding &&
                Encoding == other.Encoding &&
                SkipEmptySections == other.SkipEmptySections &&
                UseEdgePipes == other.UseEdgePipes &&
                WrapSeparator == other.WrapSeparator;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (ColumnPadding.HasValue ? ColumnPadding.Value.GetHashCode() : 0);
                hash = hash * 23 + (Encoding.HasValue ? Encoding.Value.GetHashCode() : 0);
                hash = hash * 23 + (SkipEmptySections.HasValue ? SkipEmptySections.GetHashCode() : 0);
                hash = hash * 23 + (UseEdgePipes.HasValue ? UseEdgePipes.Value.GetHashCode() : 0);
                hash = hash * 23 + (WrapSeparator != null ? WrapSeparator.GetHashCode() : 0);
                return hash;
            }
        }

        internal static MarkdownOption Combine(MarkdownOption o1, MarkdownOption o2)
        {
            return new MarkdownOption(o1)
            {
                ColumnPadding = o1.ColumnPadding ?? o2.ColumnPadding,
                Encoding = o1.Encoding ?? o2.Encoding,
                SkipEmptySections = o1.SkipEmptySections ?? o2.SkipEmptySections,
                UseEdgePipes = o1.UseEdgePipes ?? o2.UseEdgePipes,
                WrapSeparator = o1.WrapSeparator ?? o2.WrapSeparator
            };
        }

        /// <summary>
        /// Determines how table columns are padded.
        /// </summary>
        [DefaultValue(null)]
        public ColumnPadding? ColumnPadding { get; set; }

        [DefaultValue(null)]
        public MarkdownEncoding? Encoding { get; set; }

        [DefaultValue(null)]
        public bool? SkipEmptySections { get; set; }

        /// <summary>
        /// Are pipes used for the edge of tables? Single column table always require edge pipes.
        /// </summary>
        [DefaultValue(null)]
        public EdgePipeOption? UseEdgePipes { get; set; }

        [DefaultValue(null)]
        public string WrapSeparator { get; set; }
    }
}
