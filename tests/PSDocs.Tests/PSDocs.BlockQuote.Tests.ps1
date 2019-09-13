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
$dummyObject = New-Object -TypeName PSObject;

Describe 'PSDocs -- BlockQuote keyword' -Tag BlockQuote {
    Context 'Markdown' {
        It 'Should handle single line input' {
            document 'BlockQuoteSingleMarkdown' {
                'This is a single line' | BlockQuote
            }
            $result = BlockQuoteSingleMarkdown -InputObject $dummyObject -PassThru -Verbose;
            $result | Should -Match '\> This is a single line';
        }

        It 'Should handle multiline input' {
            document 'BlockQuoteMultiMarkdown' {
                @('This is the first line.'
                'This is the second line.') | BlockQuote
            }
            $result = BlockQuoteMultiMarkdown -InputObject $dummyObject -PassThru;
            $result | Should -Match '\> This is the first line.(\r|\n|\r\n)\> This is the second line.';
        }

        It 'Should add title' {
            document 'BlockQuoteTitleMarkdown' {
                'This is a single block quote' | BlockQuote -Title 'Test'
            }
            $result = BlockQuoteTitleMarkdown -InputObject $dummyObject -PassThru;
            $result | Should -Match '\> Test(\r|\n|\r\n)\> This is a single block quote';
        }

        It 'Should add info' {
            document 'BlockQuoteInfoMarkdown' {
                'This is a single block quote' | BlockQuote -Info 'Tip'
            }
            $result = BlockQuoteInfoMarkdown -InputObject $dummyObject -PassThru;
            $result | Should -Match '\> \[!TIP\](\r|\n|\r\n)> This is a single block quote';
        }
    }
}
