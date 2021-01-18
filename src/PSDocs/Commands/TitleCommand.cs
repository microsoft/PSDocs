﻿
using PSDocs.Pipeline;
using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Set, LanguageKeywords.Title)]
    internal sealed class TitleCommand : KeywordCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        [AllowNull, AllowEmptyString]
        public string Text { get; set; }

        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(Text))
            {
                GetPipeline().Writer.WarnTitleEmpty();
                return;
            }
            GetBuilder().Title(Text);
        }
    }
}
