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
New-Item $outputPath -ItemType Directory -Force | Out-Null;

$dummyObject = New-Object -TypeName PSObject;

$Global:TestVars = @{ };

Describe 'PSDocs -- Section keyword' {
    Context 'Simple Section block' {

        # Define a test document with a section block
        document 'SectionBlockTests' {
            Section 'Test' {
                'Content'
            }
        }

        Mock -CommandName 'VisitSection' -ModuleName 'Markdown' -Verifiable -MockWith {
            param (
                $InputObject
            )

            $Global:TestVars['VisitSection'] = $InputObject;
        }

        $result = SectionBlockTests -InstanceName 'Section' -InputObject $dummyObject -OutputPath $outputPath -PassThru;

        It 'Should process Section keyword' {
            Assert-MockCalled -CommandName 'VisitSection' -ModuleName 'Markdown' -Times 1;
        }

        It 'Should be Section object' {
            $Global:TestVars['VisitSection'].Type | Should be 'Section';
        }

        It 'Should have expected section name' {
            $Global:TestVars['VisitSection'].Content | Should be 'Test';
        }

        It 'Should have expected section level' {
            $Global:TestVars['VisitSection'].Level | Should be 2;
        }
    }

    Context 'Section markdown' {

        # Define a test document with a section block
        document 'SectionBlockTests' {
            Section 'SingleLine' {
                'This is a single line markdown section.'
            }

            Section 'MultiLine' {
                "This is a multiline`r`ntest."
            }
        }

        $outputDoc = "$outputPath\Section.md";
        SectionBlockTests -InstanceName 'Section' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '## SingleLine(\n|\r){1,2}This is a single line markdown section.(\n|\r){4}## MultiLine(\n|\r){1,2}This is a multiline(\n|\r){1,2}test.';
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
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should contain Section 2' {
            Get-Content -Path $outputDoc -Raw | Should match '## Section 2\r\nContent 2';
        }

        It 'Should not contain Section 1' {
            Get-Content -Path $outputDoc -Raw | Should not match '## Section 1\r\nContent 1';
        }
    }
}
