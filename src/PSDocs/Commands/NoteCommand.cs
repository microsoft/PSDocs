using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Format, LanguageKeywords.Note)]
    internal sealed class NoteCommand : BlockQuoteCommandBase
    {
        private const string InfoString = "NOTE";

        protected override string GetInfo()
        {
            return InfoString;
        }
    }
}
