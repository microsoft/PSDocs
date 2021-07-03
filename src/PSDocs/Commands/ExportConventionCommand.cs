// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Definitions.Conventions;
using PSDocs.Runtime;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsLifecycle.Register, LanguageKeywords.Convention)]
    internal sealed class ExportConventionCommand : PSCmdlet
    {
        private const string InvokeCmdletName = "Invoke-PSDocumentConvention";
        private const string InvokeCmdlet_BodyParameter = "Body";

        /// <summary>
        /// Convention name.
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        [ValidateNotNullOrEmpty()]
        public string Name { get; set; }

        /// <summary>
        /// Begin block.
        /// </summary>
        [Parameter(Mandatory = false)]
        public ScriptBlock Begin { get; set; }

        /// <summary>
        /// Process block.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1)]
        public ScriptBlock Process { get; set; }

        /// <summary>
        /// End block.
        /// </summary>
        [Parameter(Mandatory = false)]
        public ScriptBlock End { get; set; }

        protected override void ProcessRecord()
        {
            var context = RunspaceContext.CurrentThread;
            var source = context.Source;

            // Return convention
            var result = new ScriptBlockDocumentConvention(
                source: source.File,
                name: Name,
                begin: ConventionBlock(context, Begin),
                process: ConventionBlock(context, Process),
                end: ConventionBlock(context, End)
            );
            WriteObject(result);
        }

        private static LanguageScriptBlock ConventionBlock(RunspaceContext context, ScriptBlock block)
        {
            if (block == null)
                return null;

            // Create PS instance for execution
            var ps = context.NewPowerShell();
            ps.AddCommand(new CmdletInfo(InvokeCmdletName, typeof(InvokeConventionCommand)));
            ps.AddParameter(InvokeCmdlet_BodyParameter, block);
            return new LanguageScriptBlock(ps);
        }
    }
}
