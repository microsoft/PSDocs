#
# Unit tests for the Note keyword
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

Describe 'PSDocs -- Note keyword' -Tag Note {
    Context 'Markdown' {
        It 'Should handle single line input' {
            document 'NoteSingleMarkdown' {
                'This is a single line' | Note
            }
            $result = NoteSingleMarkdown -PassThru;
            $result | Should -Match '\> \[\!NOTE\](\r|\n|\r\n)> This is a single line';
        }

        It 'Should handle multiline input' {
            document 'NoteMultiMarkdown' {
                @('This is the first line.'
                'This is the second line.') | Note
            }
            $result = NoteMultiMarkdown -PassThru;
            $result | Should -Match '\> \[\!NOTE\](\r|\n|\r\n)> This is the first line\.(\r|\n|\r\n)> This is the second line\.';
        }

        It 'Should handle script block input' {
            document 'NoteScriptBlockMarkdown' {
                Note {
                    'This is a single line'
                }
            }
            $result = NoteScriptBlockMarkdown -PassThru;
            $result | Should -Match '\> \[\!NOTE\](\r|\n|\r\n)> This is a single line';
        }
    }
}
