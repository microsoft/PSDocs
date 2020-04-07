using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Set, LanguageKeywords.Title)]
    internal sealed class TitleCommand : KeywordCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string Text { get; set; }

        protected override void BeginProcessing()
        {
            GetBuilder().Title(Text);
        }
    }
}
