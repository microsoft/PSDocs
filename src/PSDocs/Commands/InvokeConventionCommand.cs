
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, LanguageKeywords.Convention)]
    internal sealed class InvokeConventionCommand : PSCmdlet
    {
        [Parameter()]
        public string[] Type;

        [Parameter()]
        public ScriptBlock If;

        [Parameter()]
        public ScriptBlock Body;

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject;

        protected override void ProcessRecord()
        {
            try
            {
                if (Body == null)
                    return;

                WriteObject(Body.Invoke(InputObject), true);
            }
            finally
            {

            }
        }
    }
}
