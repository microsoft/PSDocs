#
# Unit tests for the Warning keyword
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

Describe 'PSDocs -- Warning keyword' -Tag Warning {
    Context 'Markdown' {
        It 'Should handle single line input' {
            document 'WarningSingleMarkdown' {
                'This is a single line' | Warning
            }
            $result = WarningSingleMarkdown -PassThru;
            $result | Should -Match '\> \[\!WARNING\](\r|\n|\r\n)> This is a single line';
        }

        It 'Should handle multiline input' {
            document 'WarningMultiMarkdown' {
                @('This is the first line.'
                'This is the second line.') | Warning
            }
            $result = WarningMultiMarkdown -PassThru;
            $result | Should -Match '\> \[\!WARNING\](\r|\n|\r\n)> This is the first line\.(\r|\n|\r\n)> This is the second line\.';
        }

        It 'Should handle script block input' {
            document 'WarningScriptBlockMarkdown' {
                Warning {
                    'This is a single line'
                }
            }
            $result = WarningScriptBlockMarkdown -PassThru;
            $result | Should -Match '\> \[\!WARNING\](\r|\n|\r\n)> This is a single line';
        }
    }
}
