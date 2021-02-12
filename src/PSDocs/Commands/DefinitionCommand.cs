
using PSDocs.Runtime;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.New, LanguageKeywords.Definition)]
    internal sealed class DefinitionCommand : PSCmdlet
    {
        private const string InvokeBlockCmdletName = "Invoke-Block";
        private const string InvokeBlockCmdlet_BodyParameter = "Body";

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

        protected override void ProcessRecord()
        {
            var context = RunspaceContext.CurrentThread;
            var sourceFile = context.SourceFile;
            var extent = new ResourceExtent(
                file: sourceFile.Path,
                startLineNumber: Body.Ast.Extent.StartLineNumber
            );

            // Create PS instance for execution
            var ps = context.NewPowerShell();
            ps.AddCommand(new CmdletInfo(InvokeBlockCmdletName, typeof(InvokeDocumentCommand)));
            //ps.AddParameter(InvokeBlockCmdlet_TypeParameter, NodeType);
            ps.AddParameter(InvokeBlockCmdlet_BodyParameter, Body);

            var block = new ScriptDocumentBlock(
                source: sourceFile,
                name: Name,
                //info: helpInfo,
                body: ps,
                tag: Tag,
                extent: extent
            //type: NodeType
            //dependsOn: RuleHelper.ExpandRuleName(DependsOn, MyInvocation.ScriptName, source.ModuleName),
            //configuration: Configure
            );
            WriteObject(block);
        }
    }
}
