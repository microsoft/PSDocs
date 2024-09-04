# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for the Metadata keyword
#

[CmdletBinding()]
param (

)
BeforeAll {
    # Setup error handling
    $ErrorActionPreference = 'Stop';
    Set-StrictMode -Version latest;

    # Setup tests paths
    $rootPath = $PWD;
    Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;
    $here = (Resolve-Path $PSScriptRoot).Path;
    $dummyObject = New-Object -TypeName PSObject;
}
Describe 'PSDocs -- Metadata keyword' -Tag Metadata {

    Context 'Markdown' {
        BeforeAll{

    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Keyword.Doc.ps1';
        $invokeParams = @{
            Path        = $docFilePath
            InputObject = $dummyObject
            PassThru    = $True
        }
    }
        It 'Metadata single entry' {
            $result = Invoke-PSDocument @invokeParams -Name 'MetadataSingleEntry';
            $result | Should -Match '---(\r|\n|\r\n)title: Test(\r|\n|\r\n)---';
        }

        It 'Metadata multiple entries' {
            $result = Invoke-PSDocument @invokeParams -Name 'MetadataMultipleEntry';
            $result | Should -Match '---(\r|\n|\r\n)value1: ABC(\r|\n|\r\n)value2: EFG(\r|\n|\r\n)---';
        }

        It 'Metadata multiple blocks' {
            $result = Invoke-PSDocument @invokeParams -Name 'MetadataMultipleBlock';
            $result | Should -Match '---(\r|\n|\r\n)value1: ABC(\r|\n|\r\n)value2: EFG(\r|\n|\r\n)---';
        }

        It 'Document without Metadata block' {
            $result = Invoke-PSDocument @invokeParams -Name 'NoMetdata';
            $result | Should -Not -Match '---(\r|\n|\r\n)';
        }

        It 'Document null Metadata block' {
            $result = Invoke-PSDocument @invokeParams -Name 'NullMetdata';
            $result | Should -Not -Match '---(\r|\n|\r\n)';
        }
    }
}
