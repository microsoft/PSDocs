#
# Unit tests for the BlockQuote keyword
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

$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSDocs.Tests/BlockQuote;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

Describe 'PSDocs -- BlockQuote keyword' -Tag BlockQuote {
    Context 'Markdown' {

        It 'Should handle single line input' {
            document 'BlockQuoteSingleMarkdown' {
                'This is a single block quote' | BlockQuote
            }

            $outputDoc = "$outputPath\BlockQuoteSingleMarkdown.md";
            BlockQuoteSingleMarkdown -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should be $True;
            $outputDoc | Should -FileContentMatchMultiline '\> This is a single block quote';
        }

        It 'Should handle multiline input' {
            document 'BlockQuoteMultiMarkdown' {
                @('This is the first line.'
                'This is the second line.') | BlockQuote
            }

            $outputDoc = "$outputPath\BlockQuoteMultiMarkdown.md";
            BlockQuoteMultiMarkdown -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should -Be $True;
            $outputDoc | Should -FileContentMatchMultiline '\> This is the first line.\r\n\> This is the second line.';
        }
    }
}
