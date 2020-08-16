
using System.Collections;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Set, LanguageKeywords.Metadata)]
    internal sealed class MetadataCommand : KeywordCmdlet
    {
        [Parameter(Position = 0)]
        public IDictionary Body { get; set; }

        protected override void BeginProcessing()
        {
            if (Body == null || Body.Count == 0)
                return;

            GetBuilder().Metadata(Body);
        }
    }
}
