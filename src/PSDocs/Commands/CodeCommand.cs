using PSDocs.Models;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Format, LanguageKeywords.Code)]
    internal sealed class CodeCommand : KeywordCmdlet
    {
        private Code _Code;
        private List<string> _Content;

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "InfoString")]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "StringInfoString")]
        public string Info { get; set; }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Default", ValueFromPipeline = true)]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "InfoString", ValueFromPipeline = true)]
        public ScriptBlock Body { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "StringDefault", ValueFromPipeline = true)]
        [Parameter(Mandatory = true, ParameterSetName = "StringInfoString", ValueFromPipeline = true)]
        public string BodyString { get; set; }

        protected override void BeginProcessing()
        {
            _Code = GetBuilder().Code();
            _Code.Info = Info;
            _Content = new List<string>();
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "StringDefault" || ParameterSetName == "StringInfoString")
                _Content.Add(CleanIndent(BodyString));
            else
                _Content.Add(CleanIndent(Body.ToString()));
        }

        protected override void EndProcessing()
        {
            _Code.Content = string.Join(System.Environment.NewLine, _Content.ToArray());
            WriteObject(_Code);
            
        }

        private string CleanIndent(string value)
        {
            var match = Regex.Match(value, "^\r\n(?<indent> {1,})");
            if (match.Success)
                value = Regex.Replace(value, "\r\n {1,}", "\r\n");

            match = Regex.Match(value, "^\n(?<indent> {1,})");
            if (match.Success)
                value = Regex.Replace(value, "^\n {1,}", "\n");

            match = Regex.Match(value, "^(\r|\n|\r\n){1,}|(\r|\n|\r\n){1,}$");
            if (match.Success)
                value = Regex.Replace(value, "^(\r|\n|\r\n){1,}|(\r|\n|\r\n){1,}$", "");

            return value;
        }
    }
}
