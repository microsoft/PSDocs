using System;
using PSDocs.Configuration;
using PSDocs.Models;

namespace PSDocs.Processor.Markdown
{
    public sealed class MarkdownProcessor
    {

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

                    case DocumentNodeType.Note:

                        Note(context, documentNode as Note);

                        break;

                    case DocumentNodeType.Warning:

                        Warning(context, documentNode as Warning);

                        break;

                    case DocumentNodeType.Text:

                        Text(context, documentNode as Text);

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

        private void Warning(MarkdownProcessorContext context, Warning warning)
        {
            context.WriteLine(string.Empty);
            context.WriteLine("> [!WARNING]");

            foreach (var line in warning.Content)
            {
                context.WriteLine("> ", line);
            }
        }

        private void Note(MarkdownProcessorContext context, Note note)
        {
            context.WriteLine(string.Empty);
            context.WriteLine("> [!NOTE]");

            foreach (var line in note.Content)
            {
                context.WriteLine("> ", line);
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

        private void Table(MarkdownProcessorContext context, Table table)
        {
            if (table.Header == null || table.Header.Count == 0)
            {
                return;
            }

            context.WriteLine("");

            var headerCount = table.Header.Count;

            context.WriteLine(string.Concat("|", string.Join("|", table.Header), "|"));
            context.WriteLine(string.Concat(string.Empty.PadLeft(headerCount, 'X').Replace("X", "| --- "), "|"));

            for (var r = 0; r < table.Rows.Count; r++)
            {
                context.WriteLine(string.Concat('|', string.Join("|", WrapText(context, table.Rows[r])), "|"));
            }

            context.WriteLine("");
        }

        private string[] WrapText(MarkdownProcessorContext context, string[] text)
        {
            var separator = context.Option.Markdown.WrapSeparator;
            var result = new string[text.Length];

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i].Contains("\n") || text[i].Contains("\r"))
                {
                    result[i] = text[i].Replace("\r\n", separator).Replace("\n", separator).Replace("\r", separator);

                    continue;
                }

                result[i] = text[i];
            }

            return result;
        }
    }
}
