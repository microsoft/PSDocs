// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Configuration;
using PSDocs.Models;
using PSDocs.Runtime;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

namespace PSDocs.Processor.Markdown
{
    internal sealed class MarkdownProcessor
    {
        private const string MARKDOWN_BLOCKQUOTE = "> ";

        /// <summary>
        /// A markdown document result.
        /// </summary>
        private sealed class DocumentResult : IDocumentResult
        {
            private const string DEFAULT_EXTENSION = ".md";

            private readonly string _Markdown;

            internal DocumentResult(DocumentContext documentContext, string markdown)
            {
                _Markdown = markdown;
                InstanceName = documentContext.InstanceName;
                Culture = documentContext.Culture;
                OutputPath = PSDocumentOption.GetRootedPath(documentContext.OutputPath);
                FullName = GetFullName();
                Metadata = documentContext.Metadata;
                Data = documentContext.Data;
            }

            [DebuggerStepThrough]
            public override string ToString()
            {
                return _Markdown;
            }

            public string InstanceName { get; }

            public string Culture { get; }

            public string Extension => DEFAULT_EXTENSION;

            public string OutputPath { get; }

            public string FullName { get; }

            public OrderedDictionary Metadata { get; }

            public Hashtable Data { get; }

            private string GetFullName()
            {
                var fileName = string.Concat(InstanceName, Extension);
                return Path.Combine(OutputPath, fileName);
            }
        }

        public IDocumentResult Process(PSDocumentOption option, Document document)
        {
            if (document == null)
                return null;

            var context = new MarkdownProcessorContext(option, document);
            Document(context);
            return new DocumentResult(document.Context, markdown: context.GetString());
        }

        private static void Document(MarkdownProcessorContext context)
        {
            // Process metadata
            Metadata(context);

            if (!string.IsNullOrEmpty(context.Document.Title))
            {
                context.WriteHeaderHash(1);
                context.WriteLine(context.Document.Title);
                context.LineBreak();
            }

            foreach (var node in context.Document.Node)
            {
                Node(context, node);
            }
            context.EndDocument();
        }

        private static void Metadata(MarkdownProcessorContext context)
        {
            // Only write metadata block if there is at least one metadata property set
            if (context.Document.Metadata == null || context.Document.Metadata.Count == 0)
                return;

            context.WriteFrontMatter();
            foreach (var key in context.Document.Metadata.Keys)
            {
                context.WriteLine(key.ToString(), ": ", context.Document.Metadata[key].ToString());
            }
            context.WriteFrontMatter();
            context.LineBreak();
        }

        private static void Node(MarkdownProcessorContext context, object node)
        {
            if (node == null)
                return;

            if (node is DocumentNode documentNode)
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

        private static void String(MarkdownProcessorContext context, string node)
        {
            context.WriteLine(node);
        }

        private static void Section(MarkdownProcessorContext context, Section section)
        {
            context.WriteHeaderHash(section.Level);
            context.Write(section.Title);
            context.LineBreak();
            if (section.Node.Count > 0)
            {
                foreach (var node in section.Node)
                    Node(context, node);
            }
        }

        private static void BlockQuote(MarkdownProcessorContext context, BlockQuote node)
        {
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
            context.LineBreak();
        }

        private static void Code(MarkdownProcessorContext context, Code code)
        {
            context.WriteTripleBacktick();
            if (!string.IsNullOrEmpty(code.Info))
                context.Write(code.Info);

            context.Ending();
            context.WriteLine(code.Content);
            context.WriteTripleBacktick();
            context.LineBreak();
        }

        private static void Text(MarkdownProcessorContext context, Text text)
        {
            context.WriteLine(text.Content);
            context.LineBreak();
        }

        private static void Include(MarkdownProcessorContext context, Include include)
        {
            context.WriteLine(include.Content);
        }

        private static void Table(MarkdownProcessorContext context, Table table)
        {
            if (table.Headers == null || table.Headers.Count == 0)
                return;

            var lastHeader = table.Headers.Count - 1;
            var useEdgePipe = context.Option.Markdown.UseEdgePipes == EdgePipeOption.Always
                || table.Headers.Count == 1;
            var padColumn = context.Option.Markdown.ColumnPadding == ColumnPadding.Single
                || context.Option.Markdown.ColumnPadding == ColumnPadding.MatchHeader;

            // Write table headers
            for (var i = 0; i < table.Headers.Count; i++)
            {
                StartColumn(context, i, lastHeader, useEdgePipe, padColumn, padColumn);

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
                context.WriteSpace();

            context.WriteLine(useEdgePipe ? "|" : string.Empty);

            // Write table header separator
            for (var i = 0; i < table.Headers.Count; i++)
            {
                StartColumn(context, i, lastHeader, useEdgePipe, padColumn, padColumn);

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
                    var text = WrapText(context, table.Rows[r][c]);

                    StartColumn(context, c, lastHeader, useEdgePipe, padBeforePipe: padColumn, padAfterPipe: padColumn && (c < lastHeader || !string.IsNullOrEmpty(text)));

                    context.Write(text);

                    if (c < lastHeader)
                    {
                        var padding = 0;

                        // Pad column using column width
                        if (table.Headers[c].Width > 0 && (table.Headers[c].Width - text.Length) > 0)
                        {
                            padding = table.Headers[c].Width - text.Length;
                        }
                        // Pad column matching header
                        else if (context.Option.Markdown.ColumnPadding == ColumnPadding.MatchHeader)
                        {
                            if ((table.Headers[c].Label.Length - text.Length) > 0)
                            {
                                padding = table.Headers[c].Label.Length - text.Length;
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
            context.LineBreak();
        }

        private static void StartColumn(MarkdownProcessorContext context, int index, int last, bool useEdgePipe, bool padBeforePipe, bool padAfterPipe)
        {
            if (index > 0 && padBeforePipe)
            {
                context.WriteSpace();
            }
            if (index > 0 || useEdgePipe)
            {
                context.WritePipe();
            }
            if (padAfterPipe && useEdgePipe || index > 0 && padAfterPipe)
            {
                context.WriteSpace();
            }
        }

        private static string WrapText(MarkdownProcessorContext context, string text)
        {
            var separator = context.Option.Markdown.WrapSeparator;
            var formatted = text;

            if (text == null)
            {
                return string.Empty;
            }
            if (text.Contains("\n") || text.Contains("\r"))
            {
                formatted = text.Replace("\r\n", separator).Replace("\n", separator).Replace("\r", separator);
            }
            return formatted;
        }
    }
}
