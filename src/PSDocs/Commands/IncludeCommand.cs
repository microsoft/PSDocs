
using PSDocs.Models;
using PSDocs.Runtime;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Add, LanguageKeywords.Include)]
    internal sealed class IncludeCommand : KeywordCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string FileName { get; set; }

        [Parameter(Mandatory = false)]
        public string BaseDirectory { get; set; }

        [Parameter(Mandatory = false)]
        public string Culture { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter UseCulture { get; set; }

        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(Culture))
                Culture = RunspaceContext.CurrentThread.Culture;
        }

        protected override void EndProcessing()
        {
            WriteObject(ModelHelper.Include(BaseDirectory, Culture, FileName, UseCulture));
        }
    }
}
