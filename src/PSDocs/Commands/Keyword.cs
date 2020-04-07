using PSDocs.Data.Internal;
using PSDocs.Runtime;
using System.Management.Automation;

namespace PSDocs.Commands
{
    internal static class LanguageKeywords
    {
        public const string Definition = "Definition";
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
        public const string NewSection = "New-Section";
        public const string InvokeBlock = "Invoke-Block";
        public const string FormatCode = "Format-Code";
        public const string FormatBlockQuote = "Format-BlockQuote";
        public const string FormatNote = "Format-Note";
        public const string FormatWarning = "Format-Warning";
        public const string FormatTable = "Format-Table";
        public const string FormatList = "Format-List";
        public const string SetMetadata = "Set-Metadata";
        public const string SetTitle = "Set-Title";
    }

    internal abstract class KeywordCmdlet : PSCmdlet
    {
        protected ScriptDocumentBuilder GetBuilder()
        {
            return RunspaceContext.CurrentThread.Builder;
        }

        protected PSObject GetTargetObject()
        {
            return RunspaceContext.CurrentThread.TargetObject;
        }

        protected static bool True(object o)
        {
            return o != null && (o is bool bResult) && bResult;
        }
    }
}
