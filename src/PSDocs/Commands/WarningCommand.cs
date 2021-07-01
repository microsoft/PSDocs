// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Format, LanguageKeywords.Warning)]
    internal sealed class WarningCommand : BlockQuoteCommandBase
    {
        private const string InfoString = "WARNING";

        protected override string GetInfo()
        {
            return InfoString;
        }
    }
}
