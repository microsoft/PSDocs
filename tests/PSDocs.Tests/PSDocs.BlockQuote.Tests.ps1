#
# Unit tests for the BlockQuote keyword
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

Describe 'PSDocs -- BlockQuote keyword' -Tag BlockQuote {
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
            $result = Invoke-PSDocument @invokeParams -Name 'BlockQuoteSingleMarkdown';
            $result | Should -Match '\> This is a single line';
        }
        It 'Should handle multiline input' {
            $result = Invoke-PSDocument @invokeParams -Name 'BlockQuoteMultiMarkdown';
            $result | Should -Match '\> This is the first line.(\r|\n|\r\n)\> This is the second line.';
        }
        It 'Should add title' {
            $result = Invoke-PSDocument @invokeParams -Name 'BlockQuoteTitleMarkdown';
            $result | Should -Match '\> Test(\r|\n|\r\n)\> This is a single block quote';
        }
        It 'Should add info' {
            $result = Invoke-PSDocument @invokeParams -Name 'BlockQuoteInfoMarkdown';
            $result | Should -Match '\> \[!TIP\](\r|\n|\r\n)> This is a single block quote';
        }
    }
}
