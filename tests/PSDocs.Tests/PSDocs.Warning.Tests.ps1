# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for the Warning keyword
#

[CmdletBinding()]
param ()

BeforeAll{
# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

# Setup tests paths
$rootPath = $PWD;
Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;
$here = (Resolve-Path $PSScriptRoot).Path;
}
Describe 'PSDocs -- Warning keyword' -Tag Warning {
    

    Context 'Markdown' {
        BeforeAll{
            $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Keyword.Doc.ps1';
    $testObject = [PSCustomObject]@{
        Name = 'TestObject'
    }
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $testObject
            PassThru = $True
        }
    }
        It 'Should handle single line input' {
            $result = Invoke-PSDocument @invokeParams -Name 'WarningSingleMarkdown';
            $result | Should -Match '\> \[\!WARNING\](\r|\n|\r\n)> This is a single line';
        }
        It 'Should handle multiline input' {
            $result = Invoke-PSDocument @invokeParams -Name 'WarningMultiMarkdown';
            $result | Should -Match '\> \[\!WARNING\](\r|\n|\r\n)> This is the first line\.(\r|\n|\r\n)> This is the second line\.';
        }
    }
}
