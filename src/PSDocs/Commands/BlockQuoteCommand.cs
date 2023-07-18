// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation;
using PSDocs.Models;

namespace PSDocs.Commands
{
    internal abstract class BlockQuoteCommandBase : KeywordCmdlet
    {
        private List<string> _Content;

        #region Properties

        [Parameter()]
        public string Title { get; set; }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public string Text { get; set; }

        #endregion Properties

        protected override void BeginProcessing()
        {
            _Content = new List<string>();
        }

        protected override void ProcessRecord()
        {
            _Content.Add(Text);
        }

        protected override void EndProcessing()
        {
            try
            {
                var node = ModelHelper.BlockQuote(GetInfo(), Title);
                node.Content = _Content.ToArray();
                WriteObject(node);
            }
            finally
            {
                _Content.Clear();
            }
        }

        protected abstract string GetInfo();
    }

    [Cmdlet(VerbsCommon.Format, LanguageKeywords.BlockQuote)]
    internal sealed class BlockQuoteCommand : BlockQuoteCommandBase
    {
        [Parameter()]
        public string Info { get; set; }

        protected override string GetInfo()
        {
            return Info;
        }
    }
}
