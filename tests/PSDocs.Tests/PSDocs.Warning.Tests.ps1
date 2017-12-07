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
$rootPath = (Resolve-Path $PSScriptRoot\..\..).Path;
$here = Split-Path -Parent $MyInvocation.MyCommand.Path;
$src = ($here -replace '\\tests\\', '\\src\\') -replace '\.Tests', '';
$temp = "$here\..\..\build";
# $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.';

Import-Module $src -Force;
Import-Module $src\PSDocsProcessor\Markdown -Force;

$outputPath = "$temp\PSDocs.Tests\Warning";
New-Item $outputPath -ItemType Directory -Force | Out-Null;

$dummyObject = New-Object -TypeName PSObject;

$Global:TestVars = @{ };

Describe 'PSDocs -- Warning keyword' {
    Context 'Warning' {

        # Define a test document with a note
        document 'WarningVisitor' {
            
            Warning {
                'This is a warning'
            }
        }

        Mock -CommandName 'VisitWarning' -ModuleName 'Markdown' -Verifiable -MockWith {
            param (
                $InputObject
            )

            $Global:TestVars['VisitWarning'] = $InputObject;
        }

        Invoke-PSDocument -Name 'WarningVisitor' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should process keyword' {
            Assert-MockCalled -CommandName 'VisitWarning' -ModuleName 'Markdown' -Times 1;
        }

        It 'Should be expected type' {
            $Global:TestVars['VisitWarning'].Type | Should be 'Warning';
        }

        It 'Should have expected content' {
            $Global:TestVars['VisitWarning'].Content | Should be 'This is a warning';
        }
    }

    Context 'Warning single line markdown' {
        
        # Define a test document with a warning
        document 'WarningSingleMarkdown' {
            
            Warning {
                'This is a single line warning'
            }
        }

        $outputDoc = "$outputPath\WarningSingleMarkdown.md";
        Invoke-PSDocument -Name 'WarningSingleMarkdown' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '\> \[\!WARNING\]\r\n\> This is a single line warning';
        }
    }

    Context 'Warning multi-line markdown' {
        
        # Define a test document with a warning
        document 'WarningMultiMarkdown' {
            
            Warning {
                'This is the first line of the warning.'
                'This is the second line of the warning.'
            }
        }

        $outputDoc = "$outputPath\WarningMultiMarkdown.md";
        Invoke-PSDocument -Name 'WarningMultiMarkdown' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '\> \[\!WARNING\]\r\n\> This is the first line of the warning.\r\n\> This is the second line of the warning.';
        }
    }
}
