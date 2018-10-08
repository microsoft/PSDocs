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

$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSDocs.Tests/Warning;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

Describe 'PSDocs -- Warning keyword' -Tag Warning {
    Context 'Markdown' {

        It 'Should handle single line input' {
            document 'WarningSingleMarkdown' {
                'This is a single line' | Warning
            }

            $outputDoc = "$outputPath\WarningSingleMarkdown.md";
            WarningSingleMarkdown -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should be $True;
            $outputDoc | Should -FileContentMatchMultiline '\> \[\!WARNING\]\r\n\> This is a single line';
        }

        It 'Should handle multiline input' {
            document 'WarningMultiMarkdown' {
                @('This is the first line.'
                'This is the second line.') | Warning
            }

            $outputDoc = "$outputPath\WarningMultiMarkdown.md";
            WarningMultiMarkdown -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should -Be $True;
            $outputDoc | Should -FileContentMatchMultiline '\> \[\!WARNING\]\r\n\> This is the first line.\r\n\> This is the second line.';
        }

        It 'Should handle script block input' {
            document 'WarningScriptBlockMarkdown' {
                Warning {
                    'This is a single line'
                }
            }

            $outputDoc = "$outputPath\WarningScriptBlockMarkdown.md";
            WarningScriptBlockMarkdown -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should be $True;
            $outputDoc | Should -FileContentMatchMultiline '\> \[\!WARNING\]\r\n\> This is a single line';
        }
    }
}
