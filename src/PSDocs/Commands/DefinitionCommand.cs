// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Runtime;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.New, LanguageKeywords.Definition)]
    internal sealed class DefinitionCommand : PSCmdlet
    {
        private const string InvokeCmdletName = "Invoke-Block";
        private const string InvokeCmdlet_IfParameter = "If";
        private const string InvokeCmdlet_WithParameter = "With";
        private const string InvokeCmdlet_BodyParameter = "Body";

        /// <summary>
        /// Document block name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty()]
        public string Name { get; set; }

        /// <summary>
        /// Document block body.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public ScriptBlock Body { get; set; }

        /// <summary>
        /// Document block tags.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string[] Tag { get; set; }

        /// <summary>
        /// An optional script precondition before the Document is evaluated.
        /// </summary>
        [Parameter(Mandatory = false)]
        public ScriptBlock If { get; set; }

        /// <summary>
        /// An optional selector precondition before the Document is evaluated.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string[] With { get; set; }

        protected override void ProcessRecord()
        {
            var context = RunspaceContext.CurrentThread;
            var source = context.Source;
            var extent = new ResourceExtent(
                file: source.File.Path,
                startLineNumber: Body.Ast.Extent.StartLineNumber
            );

            // Create PS instance for execution
            var ps = context.NewPowerShell();
            ps.AddCommand(new CmdletInfo(InvokeCmdletName, typeof(InvokeDocumentCommand)));
            ps.AddParameter(InvokeCmdlet_IfParameter, If);
            ps.AddParameter(InvokeCmdlet_WithParameter, With);
            ps.AddParameter(InvokeCmdlet_BodyParameter, Body);

            var block = new ScriptDocumentBlock(
                source: source.File,
                name: Name,
                body: ps,
                tag: Tag,
                extent: extent
            );
            WriteObject(block);
        }
    }
}
