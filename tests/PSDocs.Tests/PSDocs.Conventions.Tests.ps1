# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for the PSDocs conventions
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
    $outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSDocs.Tests/Conventions;
    $here = (Resolve-Path $PSScriptRoot).Path;
}
Describe 'PSDocs -- Conventions' -Tag Conventions {


    Context '-Convention' {
        BeforeAll {
            $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Conventions.Doc.ps1';
            $testObject = [PSCustomObject]@{
                Name = 'TestObject'
            }
            $invokeParams = @{
                Path        = $docFilePath
                InputObject = $testObject
                OutputPath  = $outputPath
            }
        }
        It 'Generate output' {
            # Singe convention
            $result = Invoke-PSDocument @invokeParams -Name 'ConventionDoc1' -Convention 'TestNamingConvention1';
            $result | Should -BeLike '*TestObject_2.md';
            $testFile = Join-Path -Path $outputPath -ChildPath 'new/TestObject_2.md';
            Test-Path -Path $testFile | Should -Be $True;
            $tocFile = Join-Path -Path $outputPath -ChildPath 'new/toc.yaml';
            Test-Path -Path $tocFile | Should -Be $True;

            # Multiple conventions
            $result = Invoke-PSDocument @invokeParams -Name 'ConventionDoc1' -Convention 'TestNamingConvention1', 'TestNamingConvention2';
            $result | Should -BeLike '*TestObject_3.md';
            $testFile = Join-Path -Path $outputPath -ChildPath 'new/new/TestObject_3.md';
            Test-Path -Path $testFile | Should -Be $True;
        }
    }
}
