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
                'This is a single line' | BlockQuote
            }

            $outputDoc = "$outputPath\BlockQuoteSingleMarkdown.md";
            BlockQuoteSingleMarkdown -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should be $True;
            $outputDoc | Should -FileContentMatchMultiline '\> This is a single line';
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

        It 'Should add title' {
            document 'BlockQuoteTitleMarkdown' {
                'This is a single block quote' | BlockQuote -Title 'Test'
            }

            $outputDoc = "$outputPath\BlockQuoteTitleMarkdown.md";
            BlockQuoteTitleMarkdown -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should be $True;
            $outputDoc | Should -FileContentMatchMultiline '\> Test\r\n\> This is a single block quote';
        }

        It 'Should add info' {
            document 'BlockQuoteInfoMarkdown' {
                'This is a single block quote' | BlockQuote -Info 'Tip'
            }

            $outputDoc = "$outputPath\BlockQuoteInfoMarkdown.md";
            BlockQuoteInfoMarkdown -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should be $True;
            $outputDoc | Should -FileContentMatchMultiline '\> \[!TIP\]\r\n\> This is a single block quote';
        }
    }
}
