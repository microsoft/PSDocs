#
# Unit tests for the Metadata keyword
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

Describe 'PSDocs -- Metadata keyword' -Tag Metadata {
    Context 'Markdown' {
        It 'Metadata single entry' {
            document 'MetadataSingleEntry' {
                Metadata ([ordered]@{
                    title = 'Test'
                })
            }
            $result = MetadataSingleEntry -InputObject $dummyObject -PassThru;
            $result | Should -Match '---(\r|\n|\r\n)title: Test(\r|\n|\r\n)---';
        }

        It 'Metadata multiple entries' {
            document 'MetadataMultipleEntry' {
                Metadata ([ordered]@{
                    value1 = 'ABC'
                    value2 = 'EFG'
                })
            }
            $result = MetadataMultipleEntry -InputObject $dummyObject -PassThru
            $result | Should -Match '---(\r|\n|\r\n)value1: ABC(\r|\n|\r\n)value2: EFG(\r|\n|\r\n)---';
        }

        It 'Metadata multiple blocks' {
            document 'MetadataMultipleBlock' {
                Metadata ([ordered]@{
                    value1 = 'ABC'
                })
                Section 'Test' {
                    'A test section spliting metadata blocks.'
                }
                Metadata @{
                    value2 = 'EFG'
                }
            }
            $result = MetadataMultipleBlock -InputObject $dummyObject -PassThru;
            $result | Should -Match '---(\r|\n|\r\n)value1: ABC(\r|\n|\r\n)value2: EFG(\r|\n|\r\n)---';
        }

        It 'Document without Metadata block' {
            document 'NoMetdata' {
                Section 'Test' {
                    'A test section.'
                }
            }
            $result = NoMetdata -InputObject $dummyObject -PassThru;
            $result | Should -Not -Match '---(\r|\n|\r\n)';
        }

        It 'Document null Metadata block' {
            document 'NullMetdata' {
                Metadata $Null
                Section 'Test' {
                    'A test section.'
                }
            }
            $result = NullMetdata -InputObject $dummyObject -PassThru;
            $result | Should -Not -Match '---(\r|\n|\r\n)';
        }
    }
}
