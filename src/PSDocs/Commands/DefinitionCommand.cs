using PSDocs.Runtime;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.New, LanguageKeywords.Definition)]
    internal sealed class DefinitionCommand : PSCmdlet
    {
        private const string InvokeBlockCmdletName = "Invoke-Block";
        private const string InvokeBlockCmdlet_TypeParameter = "Type";
        private const string InvokeBlockCmdlet_IfParameter = "If";
        private const string InvokeBlockCmdlet_BodyParameter = "Body";

        /// <summary>
        /// The name of the document.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty()]
        public string Name { get; set; }

        /// <summary>
        /// The definition of the document.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public ScriptBlock Body { get; set; }

        [Parameter(Mandatory = false)]
        public string[] Tag { get; set; }

        protected override void ProcessRecord()
        {
            var context = RunspaceContext.CurrentThread;
            //var metadata = GetMetadata(MyInvocation.ScriptName, MyInvocation.ScriptLineNumber, MyInvocation.OffsetInLine);
            //var tag = GetTag(Tag);
            //var source = context.Source;

            //context.VerboseFoundRule(ruleName: Name, scriptName: MyInvocation.ScriptName);

            //var visitor = new RuleLanguageAst(ruleName: Name, context: context);
            //Body.Ast.Visit(visitor);

            //if (visitor.Errors != null)
            //{
            //    foreach (var errorRecord in visitor.Errors)
            //    {
            //        WriteError(errorRecord: errorRecord);
            //    }
            //}

            //CheckDependsOn();

            var ps = context.NewPowerShell();
            ps.AddCommand(new CmdletInfo(InvokeBlockCmdletName, typeof(InvokeBlockCommand)));
            //ps.AddParameter(InvokeBlockCmdlet_TypeParameter, NodeType);
            ps.AddParameter(InvokeBlockCmdlet_BodyParameter, Body);

            //PipelineContext.EnableLogging(ps);

            var block = new ScriptDocumentBlock(
                source: context.SourceFile,
                name: Name,
                //info: helpInfo,
                body: ps,
                tag: Tag
            //type: NodeType
            //dependsOn: RuleHelper.ExpandRuleName(DependsOn, MyInvocation.ScriptName, source.ModuleName),
            //configuration: Configure
            );
            WriteObject(block);
        }
    }
}
