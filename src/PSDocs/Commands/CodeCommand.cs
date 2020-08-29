
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
        private const string ParameterSet_StringDefault = "StringDefault";
        private const string ParameterSet_InfoString = "InfoString";
        private const string ParameterSet_StringInfoString = "StringInfoString";
        private const string ParameterSet_Default = "Default";

        private List<string> _Content;

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ParameterSet_InfoString)]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ParameterSet_StringInfoString)]
        public string Info { get; set; }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ParameterSet_Default, ValueFromPipeline = true)]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = ParameterSet_InfoString, ValueFromPipeline = true)]
        public ScriptBlock Body { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_StringDefault, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_StringInfoString, ValueFromPipeline = true)]
        public string BodyString { get; set; }

        protected override void BeginProcessing()
        {
            _Content = new List<string>();
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == ParameterSet_StringDefault || ParameterSetName == ParameterSet_StringInfoString)
                AddContent(BodyString);
            else
                AddContent(Body.ToString());
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

        private void AddContent(string input)
        {
            if (string.IsNullOrEmpty(input))
                return;

            var content = new StringContent(input);
            _Content.AddRange(content.ReadLines());
        }
    }
}
