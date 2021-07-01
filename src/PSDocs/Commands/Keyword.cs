// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Data.Internal;
using PSDocs.Pipeline;
using PSDocs.Runtime;
using System.Management.Automation;

namespace PSDocs.Commands
{
    internal static class LanguageKeywords
    {
        public const string Definition = "Definition";
        public const string Convention = "PSDocumentConvention";
        public const string Document = "Document";
        public const string Block = "Block";
        public const string Note = "Note";
        public const string Warning = "Warning";
        public const string Code = "Code";
        public const string BlockQuote = "BlockQuote";
        public const string Table = "Table";
        public const string List = "List";
        public const string Include = "Include";
        public const string Section = "Section";
        public const string Metadata = "Metadata";
        public const string Title = "Title";
    }

    internal static class LanguageCmdlets
    {
        public const string NewDefinition = "New-Definition";
        public const string ExportConvention = "Export-PSDocumentConvention";
        public const string NewSection = "New-Section";
        public const string InvokeBlock = "Invoke-Block";
        public const string InvokeConvention = "Invoke-PSDocumentConvention";
        public const string FormatCode = "Format-Code";
        public const string FormatBlockQuote = "Format-BlockQuote";
        public const string FormatNote = "Format-Note";
        public const string FormatWarning = "Format-Warning";
        public const string FormatTable = "Format-Table";
        public const string FormatList = "Format-List";
        public const string SetMetadata = "Set-Metadata";
        public const string SetTitle = "Set-Title";
        public const string AddInclude = "Add-Include";
    }

    internal abstract class KeywordCmdlet : PSCmdlet
    {
        protected static ScriptDocumentBuilder Builder
        {
            get { return RunspaceContext.CurrentThread.Builder; }
        }

        protected static ScriptDocumentBuilder GetBuilder()
        {
            return RunspaceContext.CurrentThread.Builder;
        }

        protected static PSObject GetTargetObject()
        {
            return RunspaceContext.CurrentThread.TargetObject;
        }

        protected static PipelineContext GetPipeline()
        {
            return RunspaceContext.CurrentThread.Pipeline;
        }

        protected static bool True(object o)
        {
            return o != null && TryBool(o, out bool bResult) && bResult;
        }

        protected static bool TryBool(object o, out bool value)
        {
            value = false;
            if (ObjectHelper.GetBaseObject(o) is bool result)
            {
                value = result;
                return true;
            }
            return false;
        }

        protected static bool TryString(object o, out string value)
        {
            value = null;
            if (ObjectHelper.GetBaseObject(o) is string result)
            {
                value = result;
                return true;
            }
            return false;
        }
    }
}
