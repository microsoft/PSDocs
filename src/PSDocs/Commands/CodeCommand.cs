
using PSDocs.Models;
using PSDocs.Runtime;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Format, LanguageKeywords.Code)]
    internal sealed class CodeCommand : KeywordCmdlet
    {
        private const string ParameterSet_Pipeline = "Pipeline";
        private const string ParameterSet_InfoString = "InfoString";
        private const string ParameterSet_PipelineInfoString = "PipelineInfoString";
        private const string ParameterSet_Default = "Default";

        private List<string> _Content;

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ParameterSet_InfoString)]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ParameterSet_PipelineInfoString)]
        public string Info { get; set; }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ParameterSet_Default, ValueFromPipeline = false)]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = ParameterSet_InfoString, ValueFromPipeline = false)]
        public ScriptBlock Body { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_Pipeline, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_PipelineInfoString, ValueFromPipeline = true)]
        [AllowNull]
        [AllowEmptyString]
        [AllowEmptyCollection]
        public object InputObject { get; set; }

        protected override void BeginProcessing()
        {
            _Content = new List<string>();
        }

        protected override void ProcessRecord()
        {
            var content = ParameterSetName == ParameterSet_Pipeline || ParameterSetName == ParameterSet_PipelineInfoString ? InputObject : Body;
            AddContent(content);
        }

        protected override void EndProcessing()
        {
            try
            {
                var node = ModelHelper.NewCode();
                node.Info = Info;
                node.Content = string.Join(Environment.NewLine, _Content.ToArray());
                WriteObject(node);
            }
            finally
            {
                _Content.Clear();
            }
        }

        private void AddContent(object input)
        {
            var s = input?.ToString();
            if (s == null || string.IsNullOrEmpty(s))
                return;

            var content = new StringContent(s);
            _Content.AddRange(content.ReadLines());
        }
    }
}
