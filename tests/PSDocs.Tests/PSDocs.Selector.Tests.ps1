# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for PSDocs selectors
#

[CmdletBinding()]
param ()

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
        Name = 'ObjectName'
        Value = 'ObjectValue'
        generator = 'PSDocs'
    }

    [PSObject]@{
        Name = 'HashName'
        Value = 'HashValue'
        generator = 'notPSDocs'
    }
)

Describe 'PSDocs selectors' -Tag 'Selector' {
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Selector.Doc.ps1';
    $selectorFilePath = Join-Path -Path $here -ChildPath 'Selectors.Doc.yaml';

    Context 'With -InputObject' {
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
