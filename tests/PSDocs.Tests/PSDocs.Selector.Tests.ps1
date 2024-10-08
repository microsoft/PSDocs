# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for PSDocs selectors
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

    $outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSDocs.Tests/Selector;
    Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
    $Null = New-Item -Path $outputPath -ItemType Directory -Force;
    $here = (Resolve-Path $PSScriptRoot).Path;

    $dummyObject = @(
        [PSObject]@{
            Name      = 'ObjectName'
            Value     = 'ObjectValue'
            generator = 'PSDocs'
        }

        [PSObject]@{
            Name      = 'HashName'
            Value     = 'HashValue'
            generator = 'notPSDocs'
        }
    )
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Selector.Doc.ps1';
    $selectorFilePath = Join-Path -Path $here -ChildPath 'Selectors.Doc.yaml';
}
Describe 'PSDocs selectors' -Tag 'Selector' {
    Context 'Invoke definitions' {
        BeforeAll {
        
            $invokeParams = @{
                Path = @($docFilePath, $selectorFilePath)
            }
        
            It 'Generates documentation for matching objects' {
                $result = @($dummyObject | Invoke-PSDocument @invokeParams -Name 'Selector.WithInputObject' -PassThru);
                $result | Should -Not -BeNullOrEmpty;
                $result | Should -Not -Be 'Name: HashName';
            }
        }
    }

    Context 'Get definitions' {
        It 'With selector' {
            $getParams = @{
                Path = @($docFilePath, $selectorFilePath)
            }
            $result = @(Get-PSDocument @getParams)
            $result | Should -Not -BeNullOrEmpty;
        }

        It 'Missing selector' {
            $getParams = @{
                Path = @($docFilePath)
            }
            { Get-PSDocument @getParams -ErrorAction Stop } | Should -Throw -ErrorId 'PSDocs.Parse.SelectorNotFound';
        }
    }
}
