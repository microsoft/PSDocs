// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, LanguageKeywords.Block)]
    internal sealed class InvokeDocumentCommand : PSCmdlet
    {
        [Parameter()]
        public string[] Type;

        [Parameter()]
        public ScriptBlock If;

        [Parameter()]
        public ScriptBlock Body;

        protected override void ProcessRecord()
        {
            try
            {
                if (Body == null)
                    return;

                WriteObject(Body.Invoke(), true);
            }
            finally
            {

            }
        }
    }
}
