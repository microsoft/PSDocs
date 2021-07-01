# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for the Note keyword
#

[CmdletBinding()]
param (

)

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

# Setup tests paths
$rootPath = $PWD;
Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;
$here = (Resolve-Path $PSScriptRoot).Path;

Describe 'PSDocs -- Note keyword' -Tag Note {
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Keyword.Doc.ps1';
    $testObject = [PSCustomObject]@{
        Name = 'TestObject'
    }

    Context 'Markdown' {
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $testObject
            PassThru = $True
        }
        It 'Should handle single line input' {
            $result = Invoke-PSDocument @invokeParams -Name 'NoteSingleMarkdown';
            $result | Should -Match '\> \[\!NOTE\](\r|\n|\r\n)> This is a single line';
        }
        It 'Should handle multiline input' {
            $result = Invoke-PSDocument @invokeParams -Name 'NoteMultiMarkdown';
            $result | Should -Match '\> \[\!NOTE\](\r|\n|\r\n)> This is the first line\.(\r|\n|\r\n)> This is the second line\.';
        }
    }
}
