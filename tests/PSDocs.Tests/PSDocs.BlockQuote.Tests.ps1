#
# Unit tests for the BlockQuote keyword
#

[CmdletBinding()]
param ()

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

# Setup tests paths
$rootPath = $PWD;
Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;
$here = (Resolve-Path $PSScriptRoot).Path;
$nl = [System.Environment]::NewLine;

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
            $result | Should -Match "Begin$($nl)$($nl)\> This is a single line$($nl)$($nl)End";
        }
        It 'Should handle multiline input' {
            $result = Invoke-PSDocument @invokeParams -Name 'BlockQuoteMultiMarkdown';
            $result | Should -Match "Begin$($nl)$($nl)\> This is the first line.$($nl)\> This is the second line.$($nl)$($nl)End";
        }
        It 'Should add title' {
            $result = Invoke-PSDocument @invokeParams -Name 'BlockQuoteTitleMarkdown';
            $result | Should -Match "Begin$($nl)$($nl)\> Test$($nl)\> This is a single block quote$($nl)$($nl)End";
        }
        It 'Should add info' {
            $result = Invoke-PSDocument @invokeParams -Name 'BlockQuoteInfoMarkdown';
            $result | Should -Match "Begin$($nl)$($nl)\> \[!TIP\]$($nl)\> This is a single block quote$($nl)$($nl)End";
        }
    }
}
