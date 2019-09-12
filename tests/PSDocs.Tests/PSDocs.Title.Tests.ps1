#
# Unit tests for the Title keyword
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

Describe 'PSDocs -- Title keyword' -Tag Title {
    Context 'Markdown' {
        It 'With single title' {
            document 'SingleTitle' {
                Title 'Test title'
            }
            $result = SingleTitle -InputObject $dummyObject -PassThru;
            $result | Should -Match "^(`# Test title(\r|\n|\r\n))";
        }

        It 'With multiple titles' {
            document 'MultipleTitle' {
                Title 'Title 1'
                Title 'Title 2'
            }
            $result = MultipleTitle -InputObject $dummyObject -PassThru;
            $result | Should -Match "^(`# Title 2(\r|\n|\r\n))";
        }
    }
}
