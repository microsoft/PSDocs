using PSDocs.Data.Internal;
using PSDocs.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSDocs.Commands
{
    internal abstract class BlockQuoteCommandBase : KeywordCmdlet
    {
        private ScriptDocumentBuilder _Builder;
        private BlockQuote _BlockQuote;
        private List<string> _Content;

        [Parameter()]
        public string Title { get; set; }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public string Text { get; set; }

        protected override void BeginProcessing()
        {
            _Builder = GetBuilder();
            _BlockQuote = GetBuilder().BlockQuote(GetInfo(), Title);
            _Content = new List<string>();
        }

        protected override void ProcessRecord()
        {
            _Content.Add(Text);
        }

        protected override void EndProcessing()
        {
            _BlockQuote.Content = _Content.ToArray();
            _Content.Clear();
            WriteObject(_BlockQuote);
        }

        protected abstract string GetInfo();
    }

    [Cmdlet(VerbsCommon.Format, LanguageKeywords.BlockQuote)]
    internal sealed class BlockQuoteCommand : BlockQuoteCommandBase
    {
        [Parameter()]
        public string Info { get; set; }

        protected override string GetInfo()
        {
            return Info;
        }
    }
}
