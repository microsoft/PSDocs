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
$rootPath = (Resolve-Path $PSScriptRoot\..\..).Path;
$here = Split-Path -Parent $MyInvocation.MyCommand.Path;
$temp = "$here\..\..\build";

Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs") -Force;
Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs/PSDocsProcessor/Markdown") -Force;

$outputPath = "$temp\PSDocs.Tests\Title";
New-Item $outputPath -ItemType Directory -Force | Out-Null;

$dummyObject = New-Object -TypeName PSObject;

$Global:TestVars = @{ };

Describe 'PSDocs -- Title keyword' {
    Context 'Title' {

        # Define a test document with a title
        document 'VisitTitle' {
            
            Title 'Test title'
        }

        Mock -CommandName 'VisitTitle' -ModuleName 'Markdown' -Verifiable -MockWith {
            param (
                $InputObject
            )

            $Global:TestVars['VisitTitle'] = $InputObject;
        }

        VisitTitle -InstanceName 'VisitTitle' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should process Title keyword' {
            Assert-MockCalled -CommandName 'VisitTitle' -ModuleName 'Markdown' -Times 1;
        }

        It 'Should be Title object' {
            $Global:TestVars['VisitTitle'].Type | Should be 'Document';
        }

        It 'Should have expected content' {
            $Global:TestVars['VisitTitle'].Title | Should match 'Test title';
        }
    }

    Context 'Single title markdown' {
        
        # Define a test document with a title
        document 'SingleTitle' {
            
            Title 'Test title'
        }

        $outputDoc = "$outputPath\SingleTitle.md";
        SingleTitle -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '^(\# Test title\r\n)';
        }
    }

    Context 'Multiple title markdown' {
        
        # Define a test document with multiple titles
        document 'MultipleTitle' {
            
            Title 'Title 1'

            Title 'Title 2'
        }

        $outputDoc = "$outputPath\MultipleTitle.md";
        MultipleTitle -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '^(\# Title 2\r\n)';
        }
    }
}
