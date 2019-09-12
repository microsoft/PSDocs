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

$dummyObject = New-Object -TypeName PSObject;

Describe 'PSDocs -- Section keyword' -Tag Section {
    Context 'Markdown' {
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

        It 'With defaults' {
            $result = SectionBlockTests -InstanceName 'Section' -InputObject $dummyObject -PassThru;
            $result | Should -Match "`#`# SingleLine(\r|\n|\r\n){2,}This is a single line markdown section.(\r|\n|\r\n){2,}`#`# MultiLine(\r|\n|\r\n){2,}This is a multiline(\r|\n|\r\n)test.";
            $result | Should -Not -Match "`#`# Empty"
            $result | Should -Match "`#`# Forced"
        }

        It 'With -Force' {
            $result = SectionBlockTests -InstanceName 'Section2' -InputObject $dummyObject -PassThru -Option @{ 'Markdown.SkipEmptySections' = $False };
            $result | Should -Match "`#`# Empty"
        }

        It 'With -If' {
            document 'SectionWhen' {
                Section 'Section 1' -If { $False } {
                    'Content 1'
                }
                Section 'Section 2' -If { $True } {
                    'Content 2'
                }
                # Support for When alias of If
                Section 'Section 3' -When { $True } {
                    'Content 3'
                }
            }
            $result = SectionWhen -InputObject $dummyObject -PassThru;
            $result | Should -Match "`#`# Section 2(\r|\n|\r\n){2,}Content 2";
            $result | Should -Match "`#`# Section 3(\r|\n|\r\n){2,}Content 3";
            $result | Should -Not -Match "`#`# Section 1(\r|\n|\r\n){2,}Content 1";
        }
    }
}
