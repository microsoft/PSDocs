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

        private LineEnding _Ending;

        public MarkdownProcessorContext(PSDocumentOption option, Document document)
        {
            Option = option;
            Document = document;
            Builder = new StringBuilder();
            _Ending = LineEnding.None;
        }

        internal enum LineEnding : byte
        {
            None = 0,
            Normal = 1,
            LineBreak = 2
        }

        public void EndDocument()
        {
            LineBreak();
        }

        public void LineBreak()
        {
            Ending(shouldBreak: true);
        }

        public void Ending(bool shouldBreak = false)
        {
            if (_Ending == LineEnding.LineBreak || (_Ending == LineEnding.Normal && !shouldBreak))
                return;

            if (shouldBreak && _Ending == LineEnding.None)
                Builder.Append(Environment.NewLine);

            Builder.Append(Environment.NewLine);
            _Ending = shouldBreak ? LineEnding.LineBreak : LineEnding.Normal;
        }

        public void WriteLine(params string[] line)
        {
            if (line == null || line.Length == 0)
                return;

            for (var i = 0; i < line.Length; i++)
            {
                Write(line[i]);
            }
            Ending();
        }

        public void Write(string text)
        {
            _Ending = LineEnding.None;
            Builder.Append(text);
        }

        private void Write(char c)
        {
            _Ending = LineEnding.None;
            Builder.Append(c);
        }

        internal void WriteSpace(int count = 1)
        {
            if (count == 0)
                return;

            Write(new string(Space, count));
        }

        internal void WritePipe()
        {
            Write(Pipe);
        }

        public void Write(char c, int count)
        {
            if (count == 0)
                return;

            Write(new string(c, count));
        }
    }
}
