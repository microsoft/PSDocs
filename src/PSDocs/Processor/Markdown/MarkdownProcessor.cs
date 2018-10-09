using System;
using System.IO;
using PSDocs.Configuration;
using PSDocs.Models;

namespace PSDocs.Processor.Markdown
{
    public sealed class MarkdownProcessor
    {
        private const string MARKDOWN_BLOCKQUOTE = "> ";

        public string Process(PSDocumentOption option, Document document)
        {
            if (document == null)
            {
                return string.Empty;
            }

            var context = new MarkdownProcessorContext(option, document);

            Document(context);

            return context.Builder.ToString();
        }

        private void Document(MarkdownProcessorContext context)
        {
            // Process metadata
            Metadata(context);

            if (!string.IsNullOrEmpty(context.Document.Title))
            {
                context.WriteLine("# ", context.Document.Title);
            }

            foreach (var node in context.Document.Node)
            {
                Node(context, node);
            }
        }

        private void Metadata(MarkdownProcessorContext context)
        {
            // Only write metadata block if there is at least one metadata property set
            if (context.Document.Metadata == null || context.Document.Metadata.Count == 0)
            {
                return;
            }

            context.WriteLine("---");

            foreach (var key in context.Document.Metadata.Keys)
            {
                context.WriteLine(key.ToString(), ": ", context.Document.Metadata[key].ToString());
            }

            context.WriteLine("---");
        }

        private void Node(MarkdownProcessorContext context, object node)
        {
            if (node == null)
            {
                return;
            }

            var documentNode = node as DocumentNode;

            if (documentNode != null)
            {
                switch (documentNode.Type)
                {
                    case DocumentNodeType.Section:

                        Section(context, documentNode as Section);

                        break;

                    case DocumentNodeType.Table:

                        Table(context, documentNode as Table);

                        break;

                    case DocumentNodeType.Code:

                        Code(context, documentNode as Code);

                        break;

                    case DocumentNodeType.BlockQuote:

                        BlockQuote(context, documentNode as BlockQuote);

                        break;

                    case DocumentNodeType.Text:

                        Text(context, documentNode as Text);

                        break;

                    case DocumentNodeType.Include:

                        Include(context, documentNode as Include);

                        break;
                }

                return;
            }

            String(context, node.ToString());
        }

        private void String(MarkdownProcessorContext context, string node)
        {
            context.WriteLine(node);
        }

        private void Section(MarkdownProcessorContext context, Section section)
        {
            var sectionPadding = string.Empty.PadLeft(section.Level, '#');

            context.WriteLine(string.Empty);
            context.WriteLine(sectionPadding, " ", section.Title);

            if (section.Node.Count > 0)
            {
                context.WriteLine(string.Empty);

                foreach (var node in section.Node)
                {
                    Node(context, node);
                }
            }
        }

        private void BlockQuote(MarkdownProcessorContext context, BlockQuote node)
        {
            context.WriteLine(string.Empty);

            if (!string.IsNullOrEmpty(node.Info))
            {
                context.Write(MARKDOWN_BLOCKQUOTE);
                context.Write("[!");
                context.Write(node.Info.ToUpper());
                context.WriteLine("]");
            }

            if (!string.IsNullOrEmpty(node.Title))
            {
                context.Write(MARKDOWN_BLOCKQUOTE);
                context.WriteLine(node.Title);
            }

            foreach (var line in node.Content)
            {
                context.Write(MARKDOWN_BLOCKQUOTE);
                context.WriteLine(line);
            }
        }

        private void Code(MarkdownProcessorContext context, Code code)
        {
            if (string.IsNullOrEmpty(code.Info))
            {
                context.WriteLine("```");
            }
            else
            {
                context.WriteLine("```", code.Info);
            }

            context.WriteLine(code.Content);

            context.WriteLine("```");
        }

        private void Text(MarkdownProcessorContext context, Text text)
        {
            context.WriteLine(text.Content);
        }

        private void Include(MarkdownProcessorContext context, Include include)
        {
            var text = File.ReadAllText(include.Path);
            context.WriteLine(text);
        }

        private void Table(MarkdownProcessorContext context, Table table)
        {
            if (table.Headers == null || table.Headers.Count == 0)
            {
                return;
            }

            context.WriteLine(string.Empty);

            var lastHeader = table.Headers.Count - 1;
            var useEdgePipe = context.Option.Markdown.UseEdgePipes == EdgePipeOption.Always
                || table.Headers.Count == 1;
            var padColumn = context.Option.Markdown.ColumnPadding == ColumnPadding.Single
                || context.Option.Markdown.ColumnPadding == ColumnPadding.MatchHeader;

            // Write table headers
            for (var i = 0; i < table.Headers.Count; i++)
            {
                StartColumn(context, i, lastHeader, useEdgePipe, padColumn);
                
                context.Write(table.Headers[i].Label);

                if (i < lastHeader)
                {
                    var padding = 0;

                    // Pad column
                    if (table.Headers[i].Width > 0 && (table.Headers[i].Width - table.Headers[i].Label.Length) > 0)
                    {
                        padding = table.Headers[i].Width - table.Headers[i].Label.Length;
                    }

                    context.WriteSpace(padding);
                }
            }

            if (padColumn && useEdgePipe)
            {
                context.WriteSpace();
            }

            context.WriteLine(useEdgePipe ? "|" : string.Empty);

            // Write table header separator
            for (var i = 0; i < table.Headers.Count; i++)
            {
                StartColumn(context, i, lastHeader, useEdgePipe, padColumn);

                switch (table.Headers[i].Alignment)
                {
                    case Alignment.Left:
                        context.Write(":");
                        context.Write('-', table.Headers[i].Label.Length - 1);

                        break;

                    case Alignment.Right:
                        context.Write('-', table.Headers[i].Label.Length - 1);
                        context.Write(":");

                        break;

                    case Alignment.Center:
                        context.Write(":");
                        context.Write('-', table.Headers[i].Label.Length - 2);
                        context.Write(":");

                        break;

                    default:
                        context.Write('-', table.Headers[i].Label.Length);

                        break;
                }

                if (i < lastHeader)
                {
                    var padding = 0;

                    // Pad column
                    if (table.Headers[i].Width > 0 && (table.Headers[i].Width - table.Headers[i].Label.Length) > 0)
                    {
                        padding = table.Headers[i].Width - table.Headers[i].Label.Length;
                    }

                    context.WriteSpace(padding);
                }
            }

            if (padColumn && useEdgePipe)
            {
                context.WriteSpace();
            }

            context.WriteLine(useEdgePipe ? "|" : string.Empty);

            // Write table rows
            for (var r = 0; r < table.Rows.Count; r++)
            {
                for (var c = 0; c < table.Rows[r].Length; c++)
                {
                    StartColumn(context, c, lastHeader, useEdgePipe, padColumn);

                    var text = WrapText(context, table.Rows[r][c]);

                    context.Write(text);

                    if (c < lastHeader)
                    {
                        var padding = 0;

                        // Pad column using column width
                        if (table.Headers[c].Width > 0 && (table.Headers[c].Width - table.Rows[r][c].Length) > 0)
                        {
                            padding = table.Headers[c].Width - table.Rows[r][c].Length;
                        }
                        // Pad column matching header
                        else if (context.Option.Markdown.ColumnPadding == ColumnPadding.MatchHeader)
                        {
                            if ((table.Headers[c].Label.Length - table.Rows[r][c].Length) > 0)
                            {
                                padding = table.Headers[c].Label.Length - table.Rows[r][c].Length;
                            }
                        }

                        context.WriteSpace(padding);
                    }
                }

                if (padColumn && useEdgePipe)
                {
                    context.WriteSpace();
                }

                context.WriteLine(useEdgePipe ? "|" : string.Empty);
            }

            context.WriteLine(string.Empty);
        }

        private void StartColumn(MarkdownProcessorContext context, int index, int last, bool useEdgePipe, bool padColumn)
        {
            if (index > 0 && padColumn)
            {
                context.WriteSpace();
            }

            if (index > 0 || useEdgePipe)
            {
                context.WritePipe();
            }

            if (padColumn && useEdgePipe || index > 0 && padColumn)
            {
                context.WriteSpace();
            }
        }

        private string WrapText(MarkdownProcessorContext context, string text)
        {
            var separator = context.Option.Markdown.WrapSeparator;
            var formatted = text;

            if (text.Contains("\n") || text.Contains("\r"))
            {
                formatted = text.Replace("\r\n", separator).Replace("\n", separator).Replace("\r", separator);
            }

            return formatted;
        }
    }
}
