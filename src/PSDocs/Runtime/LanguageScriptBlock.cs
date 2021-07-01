// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;

namespace PSDocs.Runtime
{
    internal sealed class LanguageScriptBlock
    {
        private readonly PowerShell _Block;

        public LanguageScriptBlock(PowerShell block)
        {
            _Block = block;
        }

        public void Invoke()
        {
            _Block.Invoke();
        }
    }
}
