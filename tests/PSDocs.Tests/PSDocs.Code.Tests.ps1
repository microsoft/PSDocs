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
            $result | Should -Match "`# This is a comment$([System.Environment]::NewLine)This is code$([System.Environment]::NewLine)$([System.Environment]::NewLine)`# Another comment$([System.Environment]::NewLine)And code";
        }
        It 'Code markdown with named format' {
            $result = Invoke-PSDocument @invokeParams -Name 'CodeMarkdownNamedFormat';
            $result | Should -Match "``````powershell$([System.Environment]::NewLine)Get-Content$([System.Environment]::NewLine)``````";
        }
        It 'Code markdown with evaluation' {
            $result = Invoke-PSDocument @invokeParams -Name 'CodeMarkdownEval';
            $result | Should -Match "``````powershell$([System.Environment]::NewLine)2$([System.Environment]::NewLine)``````";
        }
    }
}
