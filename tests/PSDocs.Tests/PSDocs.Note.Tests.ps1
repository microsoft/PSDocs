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
$rootPath = (Resolve-Path $PSScriptRoot\..\..).Path;
$here = Split-Path -Parent $MyInvocation.MyCommand.Path;
$temp = "$here\..\..\build";

Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs") -Force;

$outputPath = "$temp\PSDocs.Tests\Note";
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction SilentlyContinue;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

$dummyObject = New-Object -TypeName PSObject;

$Global:TestVars = @{ };

Describe 'PSDocs -- Note keyword' {
    Context 'Note single line markdown' {

        # Define a test document with a note
        document 'NoteSingleMarkdown' {
            Note {
                'This is a single line note'
            }
        }

        $outputDoc = "$outputPath\NoteSingleMarkdown.md";
        NoteSingleMarkdown -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            $outputDoc | Should -FileContentMatchMultiline '\> \[\!NOTE\]\r\n\> This is a single line note';
        }
    }

    Context 'Note multi-line markdown' {

        # Define a test document with a note
        document 'NoteMultiMarkdown' {
            Note {
                'This is the first line of the note.'
                'This is the second line of the note.'
            }
        }

        $outputDoc = "$outputPath\NoteMultiMarkdown.md";
        NoteMultiMarkdown -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should -Be $True;
        }

        It 'Should match expected format' {
            $outputDoc | Should -FileContentMatchMultiline '\> \[\!NOTE\]\r\n\> This is the first line of the note.\r\n\> This is the second line of the note.';
        }
    }
}
