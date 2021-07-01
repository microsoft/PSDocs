// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSDocs.Data
{
    /// <summary>
    /// A convertable object for code content.
    /// </summary>
    public sealed class CodeContent
    {
        public string Content { get; }

        public CodeContentType Type { get; }

        private CodeContent(string content, CodeContentType type)
        {
            Content = content;
            Type = type;
        }

        private CodeContent(object value)
        {
            Type = CodeContentType.Object;
        }

        public static implicit operator CodeContent(ScriptBlock content)
        {
            return FromScriptBlock(content);
        }

        public static implicit operator CodeContent(string content)
        {
            return FromString(content);
        }

        public static implicit operator CodeContent(PSObject content)
        {
            if (content == null)
                return null;

            if (content.BaseObject is ScriptBlock sb)
                return FromScriptBlock(sb);

            if (content.BaseObject is string s)
                return FromString(s);

            return FromObject(content);
        }

        public static CodeContent FromScriptBlock(ScriptBlock content)
        {
            if (content == null)
                return null;

            return new CodeContent(content.ToString(), CodeContentType.ScriptBlock);
        }

        public static CodeContent FromString(string content)
        {
            return new CodeContent(content, CodeContentType.String);
        }

        public static CodeContent FromObject(object content)
        {
            return new CodeContent(content);
        }

        public enum CodeContentType
        {
            String,

            ScriptBlock,

            Object
        }
    }
}
