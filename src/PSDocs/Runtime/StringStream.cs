// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSDocs.Runtime
{
    internal sealed class StringStream
    {
        private const char EmptyChar = '\0';
        private const char LF = '\n';
        private const char CR = '\r';

        private readonly string _Input;
        private readonly int _LastIndex;

        private int _Position = -1;
        private char _Current = EmptyChar;

        public bool EOF => _Position > _LastIndex;

        public StringStream(string input)
        {
            _Input = input;
            _LastIndex = input.Length - 1;
            Next();
        }

        internal void SkipLineEnding()
        {
            if (!IsLineEnding(_Current))
                return;

            if (_Current == CR) // \r
                Next();

            if (_Current == LF) // \n
                Next();
        }

        public bool UntilLineEnding(out string text)
        {
            text = string.Empty;
            if (EOF)
                return false;

            var count = 0;
            var startingPos = _Position;
            while (!EOF && !IsLineEnding(_Current))
            {
                count++;
                Next();
            }
            text = _Input.Substring(startingPos, count);
            return true;
        }

        public int GetIndent()
        {
            var offset = 0;
            while (Peak(offset, out char c) && IsIndent(c))
                offset++;

            return offset;
        }

        public bool Next()
        {
            if (_Position > _LastIndex)
                return false;

            _Position++;
            if (_Position > _LastIndex)
            {
                _Current = EmptyChar;
                return false;
            }
            _Current = _Input[_Position];
            return true;
        }

        private bool Peak(int offset, out char c)
        {
            c = EmptyChar;
            var index = _Position + offset;
            if (index > _LastIndex)
                return false;

            c = _Input[index];
            return true;
        }

        public void SkipIndent(int indent)
        {
            if (indent == 0 || EOF || IsLineEnding(_Current))
                return;

            var count = 0;
            while (count < indent && IsIndent(_Current) && Next())
                count++;
        }

        private static bool IsIndent(char c)
        {
            return char.IsWhiteSpace(c);
        }

        private static bool IsLineEnding(char c)
        {
            return c == LF || c == CR;
        }
    }
}
