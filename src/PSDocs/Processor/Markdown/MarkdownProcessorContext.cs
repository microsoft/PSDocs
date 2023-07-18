// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        private const char Hash = '#';
        private const string TripleBacktick = "```";
        private const string MARKDOWN_FRONTMATTER = "---";

        public readonly IPSDocumentOption Option;
        public readonly Document Document;
        private readonly StringBuilder Builder;

        private LineEnding _Ending;

        internal MarkdownProcessorContext(IPSDocumentOption option, Document document)
        {
            Option = option;
            Document = document;
            Builder = new StringBuilder();
            _Ending = LineEnding.None;
        }

        internal string GetString()
        {
            return Builder.Length > 0 ? Builder.ToString() : null;
        }

        private enum LineEnding : byte
        {
            None = 0,
            Normal = 1,
            LineBreak = 2
        }

        internal void EndDocument()
        {
            Ending();
            if (_Ending == LineEnding.LineBreak)
                Builder.Remove(Builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
        }

        internal void LineBreak()
        {
            Ending(shouldBreak: true);
        }

        internal void Ending(bool shouldBreak = false)
        {
            if (_Ending == LineEnding.LineBreak || (_Ending == LineEnding.Normal && !shouldBreak))
                return;

            if (shouldBreak && _Ending == LineEnding.None)
                Builder.Append(Environment.NewLine);

            Builder.Append(Environment.NewLine);
            _Ending = shouldBreak ? LineEnding.LineBreak : LineEnding.Normal;
        }

        internal void WriteLine(params string[] line)
        {
            if (line == null || line.Length == 0)
                return;

            for (var i = 0; i < line.Length; i++)
            {
                Write(line[i]);
            }
            Ending();
        }

        internal void Write(string text)
        {
            _Ending = LineEnding.None;
            Builder.Append(text);
        }

        internal void Write(char c, int count)
        {
            if (count == 0)
                return;

            _Ending = LineEnding.None;
            Builder.Append(c, count);
        }

        internal void WriteSpace(int count = 1)
        {
            Write(Space, count);
        }

        internal void WritePipe()
        {
            Write(Pipe, 1);
        }

        internal void WriteTripleBacktick()
        {
            Write(TripleBacktick);
        }

        internal void WriteHeaderHash(int count)
        {
            Write(Hash, count);
            WriteSpace();
        }

        internal void WriteFrontMatter()
        {
            Write(MARKDOWN_FRONTMATTER);
            Ending();
        }
    }
}
