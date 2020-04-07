#
# Unit tests for the Code keyword
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

Describe 'PSDocs -- Code keyword' -Tag Code {
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Keyword.doc.ps1';
    $testObject = [PSCustomObject]@{
        Name = 'TestObject'
    }

    Context 'Markdown' {
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $testObject
            PassThru = $True
        }
        It 'Should have generated output' {
            $result = Invoke-PSDocument @invokeParams -Name 'CodeMarkdown';
            $result | Should -Match "`# This is a comment(\r|\n|\r\n)This is code(\r|\n|\r\n){2,}`# Another comment(\r|\n|\r\n)And code";
        }
        It 'Code markdown with named format' {
            $result = Invoke-PSDocument @invokeParams -Name 'CodeMarkdownNamedFormat';
            $result | Should -Match '```powershell(\r|\n|\r\n)Get-Content(\r|\n|\r\n)```';
        }
        It 'Code markdown with evaluation' {
            $result = Invoke-PSDocument @invokeParams -Name 'CodeMarkdownEval';
            $result | Should -Match '```powershell(\r|\n|\r\n)2(\r|\n|\r\n)```';
        }
    }
}
