using System;
using System.Text;
using PSDocs.Configuration;
using PSDocs.Models;

namespace PSDocs.Processor.Markdown
{
    internal sealed class MarkdownProcessorContext
    {
        private const char Space = ' ';
        private const char Pipe = '|';

        public readonly PSDocumentOption Option;
        public readonly Document Document;

        public readonly StringBuilder Builder;

        public MarkdownProcessorContext(PSDocumentOption option, Document document)
        {
            Option = option;
            Document = document;

            Builder = new StringBuilder();
        }

        public void WriteLine(params string[] line)
        {
            if (line == null || line.Length == 0)
            {
                return;
            }

            if (line.Length == 1)
            {
                Builder.AppendLine(line[0]);

                return;
            }

            for (var i = 0; i < line.Length - 1; i++)
            {
                Builder.Append(line[i]);
            }

            Builder.AppendLine(line[line.Length - 1]);
        }

        public void Write(string text)
        {
            Builder.Append(text);
        }

        public void Write(char c)
        {
            Builder.Append(c);
        }

        internal void WriteSpace(int count = 1)
        {
            if (count == 0)
            {
                return;
            }

            Builder.Append(new string(Space, count));
        }

        internal void WritePipe()
        {
            Builder.Append(Pipe);
        }

        public void Write(char c, int count)
        {
            if (count == 0)
            {
                return;
            }

            Builder.Append(new string(c, count));
        }
    }
}
