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

$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSDocs.Tests/Note;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

Describe 'PSDocs -- Note keyword' -Tag Note {
    Context 'Markdown' {

        It 'Should handle single line input' {
            document 'NoteSingleMarkdown' {
                'This is a single line' | Note
            }

            $outputDoc = "$outputPath\NoteSingleMarkdown.md";
            NoteSingleMarkdown -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should be $True;
            $outputDoc | Should -FileContentMatchMultiline '\> \[\!NOTE\]\r\n\> This is a single line';
        }

        It 'Should handle multiline input' {
            document 'NoteMultiMarkdown' {
                @('This is the first line.'
                'This is the second line.') | Note
            }

            $outputDoc = "$outputPath\NoteMultiMarkdown.md";
            NoteMultiMarkdown -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should -Be $True;
            $outputDoc | Should -FileContentMatchMultiline '\> \[\!NOTE\]\r\n\> This is the first line.\r\n\> This is the second line.';
        }

        It 'Should handle script block input' {
            document 'NoteScriptBlockMarkdown' {
                Note {
                    'This is a single line'
                }
            }

            $outputDoc = "$outputPath\NoteScriptBlockMarkdown.md";
            NoteScriptBlockMarkdown -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should be $True;
            $outputDoc | Should -FileContentMatchMultiline '\> \[\!NOTE\]\r\n\> This is a single line';
        }
    }
}
