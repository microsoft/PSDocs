#
# Unit tests for the Title keyword
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

Describe 'PSDocs -- Title keyword' -Tag Title {
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
        It 'With single title' {
            $result = Invoke-PSDocument @invokeParams -Name 'SingleTitle';
            $result | Should -Match "^(`# Test title(\r|\n|\r\n))";
        }
        It 'With multiple titles' {
            $result = Invoke-PSDocument @invokeParams -Name 'MultipleTitle';
            $result | Should -Match "^(`# Title 2(\r|\n|\r\n))";
        }
    }
}
