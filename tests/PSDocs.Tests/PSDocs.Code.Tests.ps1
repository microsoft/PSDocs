#
# Unit tests for the Code keyword
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

Describe 'PSDocs -- Code keyword' -Tag Code {
    Context 'Markdown' {
        It 'Should have generated output' {
            document 'CodeMarkdown' {
                Code {
                    # This is a comment
                    This is code

                    # Another comment
                    And code
                }
            }
            $result = CodeMarkdown -InputObject $dummyObject -PassThru;
            $result | Should -Match "`# This is a comment(\r|\n|\r\n)This is code(\r|\n|\r\n){2,}`# Another comment(\r|\n|\r\n)And code";
        }

        It 'Code markdown with named format' {
            document 'CodeMarkdownNamedFormat' {
                Code powershell {
                    Get-Content
                }
            }
            $result = CodeMarkdownNamedFormat -InputObject $dummyObject -PassThru;
            $result | Should -Match '```powershell(\r|\n|\r\n)Get-Content(\r|\n|\r\n)```';
        }

        It 'Code markdown with evaluation' {
            document 'CodeMarkdownEval' {
                $a = 1; $a += 1; $a | Code powershell;
            }
            $result = CodeMarkdownEval -InputObject $dummyObject -PassThru;
            $result | Should -Match '```powershell(\r|\n|\r\n)2(\r|\n|\r\n)```';
        }
    }
}
