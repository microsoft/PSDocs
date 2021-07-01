# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for the Code keyword
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

Describe 'PSDocs -- Code keyword' -Tag Code {
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
        It 'Should have generated output' {
            $result = Invoke-PSDocument @invokeParams -Name 'CodeMarkdown';
            $result | Should -Match "Begin$($nl)$($nl)``````powershell$($nl)`# This is a comment$($nl)This is code$($nl)$($nl)`# Another comment$($nl)And code$($nl)``````$($nl)$($nl)End";
        }
        It 'Code markdown with named format' {
            $result = Invoke-PSDocument @invokeParams -Name 'CodeMarkdownNamedFormat';
            $result | Should -Match "Begin$($nl)$($nl)``````powershell$($nl)Get-Content$($nl)``````$($nl)$($nl)End";
        }
        It 'Code markdown with evaluation' {
            $result = Invoke-PSDocument @invokeParams -Name 'CodeMarkdownEval';
            $result | Should -Match "Begin$($nl)$($nl)``````powershell$($nl)2$($nl)``````$($nl)$($nl)End";
        }
        It 'Code markdown with include' {
            $result = Invoke-PSDocument @invokeParams -Name 'CodeInclude';
            $result | Should -Match "Begin$($nl)$($nl)``````yaml$($nl)generator: PSDocs$($nl)``````$($nl)$($nl)End";
        }
        It 'Code markdown with JSON conversion' {
            $result = Invoke-PSDocument @invokeParams -Name 'CodeJson';
            $result | Should -Match "``````json$($nl){$($nl)    `"Name`": `"Value`"$($nl)}$($nl)``````";
        }
        It 'Code markdown with YAML conversion' {
            $result = Invoke-PSDocument @invokeParams -Name 'CodeYaml';
            $result | Should -Match "``````yaml$($nl)name: Value$($nl)``````";
        }
    }
}
