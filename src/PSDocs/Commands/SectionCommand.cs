﻿
using PSDocs.Runtime;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.New, LanguageKeywords.Section)]
    internal sealed class SectionCommand : KeywordCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        public ScriptBlock Body { get; set; }

        [Parameter(Mandatory = false)]
        public ScriptBlock If { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void ProcessRecord()
        {
            if (!TryCondition())
                return;

            var targetObect = InputObject ?? GetTargetObject();
            var builder = GetBuilder();
            var section = builder.EnterSection(Name);
            bool shouldWrite = true;

            try
            {
                shouldWrite = section.AddNodes(Body.Invoke()) || ShouldForce();
            }
            finally
            {
                builder.ExitSection();
            }
            if (shouldWrite)
                WriteObject(section);
        }

        private bool ShouldForce()
        {
            return Force.ToBool() || !RunspaceContext.CurrentThread.Pipeline.Option.Markdown.SkipEmptySections;
        }

        private bool TryCondition()
        {
            return If == null || True(If.InvokeReturnAsIs());
        }
    }
}
