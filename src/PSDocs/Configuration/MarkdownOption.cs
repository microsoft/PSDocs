// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSDocs.Configuration
{
    /// <summary>
    /// Options that affect markdown formatting.
    /// </summary>
    public sealed class MarkdownOption : IEquatable<MarkdownOption>
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
        /// <remarks>
        /// Defaults to MatchHeader.
        /// </remarks>
        [DefaultValue(null)]
        public ColumnPadding? ColumnPadding { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Defaults to UTF-8 without byte order mark (BOM)
        /// </remarks>
        [DefaultValue(null)]
        public MarkdownEncoding? Encoding { get; set; }

        /// <summary>
        /// Determines if empty sections are included in output.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        [DefaultValue(null)]
        public bool? SkipEmptySections { get; set; }

        /// <summary>
        /// Determines when pipes on the edge of a table should be used.
        /// </summary>
        /// <remarks>
        /// Defaults to WhenRequired.
        /// </remarks>
        [DefaultValue(null)]
        public EdgePipeOption? UseEdgePipes { get; set; }

        /// <summary>
        /// Specifies the character/ string to use when wrapping lines in a table cell.
        /// </summary>
        /// <remarks>
        /// Defaults to ' '.
        /// </remarks>
        [DefaultValue(null)]
        public string WrapSeparator { get; set; }

        internal void Load(EnvironmentHelper env)
        {
            if (env.TryEnum("PSDOCS_MARKDOWN_COLUMNPADDING", out ColumnPadding columnPadding))
                ColumnPadding = columnPadding;

            if (env.TryEnum("PSDOCS_MARKDOWN_ENCODING", out MarkdownEncoding encoding))
                Encoding = encoding;

            if (env.TryBool("PSDOCS_MARKDOWN_SKIPEMPTYSECTIONS", out bool skipEmptySections))
                SkipEmptySections = skipEmptySections;

            if (env.TryEnum("PSDOCS_MARKDOWN_USEEDGEPIPES", out EdgePipeOption useEdgePipes))
                UseEdgePipes = useEdgePipes;

            if (env.TryString("PSDOCS_MARKDOWN_WRAPSEPARATOR", out string wrapSeparator))
                WrapSeparator = wrapSeparator;
        }

        internal void Load(Dictionary<string, object> index)
        {
            if (index.TryPopEnum("Markdown.ColumnPadding", out ColumnPadding columnPadding))
                ColumnPadding = columnPadding;

            if (index.TryPopEnum("Markdown.Encoding", out MarkdownEncoding encoding))
                Encoding = encoding;

            if (index.TryPopBool("Markdown.SkipEmptySections", out bool skipEmptySections))
                SkipEmptySections = skipEmptySections;

            if (index.TryPopEnum("Markdown.UseEdgePipes", out EdgePipeOption useEdgePipes))
                UseEdgePipes = useEdgePipes;

            if (index.TryPopString("Markdown.WrapSeparator", out string wrapSeparator))
                WrapSeparator = wrapSeparator;
        }
    }
}
