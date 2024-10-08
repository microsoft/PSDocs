# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for the Title keyword
#

[CmdletBinding()]
param ()

BeforeAll {
    # Setup error handling
    $ErrorActionPreference = 'Stop';
    Set-StrictMode -Version latest;

    # Setup tests paths
    $rootPath = $PWD;
    Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;
    $here = (Resolve-Path $PSScriptRoot).Path;
}
Describe 'PSDocs -- Title keyword' -Tag Title {
    

    Context 'Markdown' {
        BeforeAll {
            $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Keyword.Doc.ps1';
            $testObject = [PSCustomObject]@{
                Name = 'TestObject'
            }
        
            $invokeParams = @{
                Path        = $docFilePath
                InputObject = $testObject
                PassThru    = $True
                ErrorAction = [System.Management.Automation.ActionPreference]::Stop
            }
        }
        It 'With single title' {
            $result = Invoke-PSDocument @invokeParams -Name 'SingleTitle';
            $result | Should -Match "^(`# Test title(\r|\n|\r\n))";
        }
        It 'With multiple titles' {
            $result = Invoke-PSDocument @invokeParams -Name 'MultipleTitle';
            $result | Should -Match "^(`# Title 2(\r|\n|\r\n))";
        }
        It 'With empty title' {
            $Null = Invoke-PSDocument @invokeParams -Name 'EmptyTitle' -WarningAction SilentlyContinue -WarningVariable outWarnings;
            $outWarnings = @($outWarnings);
            $outWarnings | Should -Not -BeNullOrEmpty;
            $outWarnings.Length | Should -Be 2;
        }
    }
}
