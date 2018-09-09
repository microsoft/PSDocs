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
$rootPath = (Resolve-Path $PSScriptRoot\..\..).Path;
$here = Split-Path -Parent $MyInvocation.MyCommand.Path;
$temp = "$here\..\..\build";

Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs") -Force;
Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs/PSDocsProcessor/Markdown") -Force;

$outputPath = "$temp\PSDocs.Tests\Section";
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction SilentlyContinue;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

$dummyObject = New-Object -TypeName PSObject;

$Global:TestVars = @{ };

Describe 'PSDocs -- Section keyword' {

    Context 'Section markdown' {

        # Define a test document with a section block
        document 'SectionBlockTests' {
            Section 'SingleLine' {
                'This is a single line markdown section.'
            }

            Section 'MultiLine' {
                "This is a multiline`r`ntest."
            }

            Section 'Empty' {

            }

            Section 'Forced' -Force {

            }
        }

        $outputDoc = "$outputPath\Section.md";
        SectionBlockTests -InstanceName 'Section' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should -Be $True;
        }

        It 'Should match expected format' {
            $outputDoc | Should -FileContentMatchMultiline '## SingleLine\r\nThis is a single line markdown section.(\n|\r){4}## MultiLine\r\nThis is a multiline(\n|\r){1,2}test.';
        }

        It 'Empty section is not present' {
            $outputDoc | Should -Not -FileContentMatch '## Empty'
        }

        It 'Forced section is present' {
            $outputDoc | Should -FileContentMatch '## Forced'
        }

        $outputDoc = "$outputPath\Section2.md";
        SectionBlockTests -InstanceName 'Section2' -InputObject $dummyObject -OutputPath $outputPath -Option @{ 'Markdown.SkipEmptySections' = $False };

        It 'Empty sections are include with option' {
            $outputDoc | Should -FileContentMatch '## Empty'
        }
    }

    Context 'Conditional section block' {

        # Define a test document with a section block
        document 'SectionWhen' {
            Section 'Section 1' -When { $False } {
                'Content 1'
            }

            Section 'Section 2' -When { $True } {
                'Content 2'
            }
        }

        $outputDoc = "$outputPath\SectionWhen.md";
        SectionWhen -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should -Be $True;
        }

        It 'Should contain Section 2' {
            $outputDoc | Should -FileContentMatchMultiline '## Section 2\r\nContent 2';
        }

        It 'Should not contain Section 1' {
            $outputDoc | Should -Not -FileContentMatchMultiline '## Section 1\r\nContent 1';
        }
    }
}
