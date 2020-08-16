#
# Unit tests for the Section keyword
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
$dummyObject = New-Object -TypeName PSObject;

Describe 'PSDocs -- Section keyword' -Tag Section {
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Keyword.Doc.ps1';

    Context 'Markdown' {
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $dummyObject
            PassThru = $True
        }

        It 'With defaults' {
            $result = Invoke-PSDocument @invokeParams -Name 'SectionBlockTests' -InstanceName 'Section';
            $result | Should -Match "`#`# SingleLine(\r|\n|\r\n){2,}This is a single line markdown section.(\r|\n|\r\n){2,}`#`# MultiLine(\r|\n|\r\n){2,}This is a multiline(\r|\n|\r\n)test.";
            $result | Should -Not -Match "`#`# Empty";
            $result | Should -Match "`#`# Forced";
        }

        It 'With -Force' {
            $result = Invoke-PSDocument @invokeParams -Name 'SectionBlockTests' -InstanceName 'Section2' -Option @{ 'Markdown.SkipEmptySections' = $False };
            $result | Should -Match "`#`# Empty";
        }

        It 'With -If' {
            $result = Invoke-PSDocument @invokeParams -Name 'SectionIf';
            $result | Should -Match "`#`# Section 2(\r|\n|\r\n){2,}Content 2";
            $result | Should -Not -Match "`#`# Section 1(\r|\n|\r\n){2,}Content 1";
        }
    }
}
