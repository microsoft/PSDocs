#
# Unit tests for the Code keyword
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

$outputPath = "$temp\PSDocs.Tests\Code";
New-Item $outputPath -ItemType Directory -Force | Out-Null;

$dummyObject = New-Object -TypeName PSObject;

$Global:TestVars = @{ };

Describe 'PSDocs -- Code keyword' {
    Context 'Code' {

        # Define a test document with a table
        document 'CodeTests' {
            
            Code {
                This is code
            }
        }

        Mock -CommandName 'VisitCode' -ModuleName 'Markdown' -Verifiable -MockWith {
            param (
                $InputObject
            )

            $Global:TestVars['VisitCode'] = $InputObject;
        }

        CodeTests -InstanceName 'Code' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should process Code keyword' {
            Assert-MockCalled -CommandName 'VisitCode' -ModuleName 'Markdown' -Times 1;
        }

        It 'Should be Code object' {
            $Global:TestVars['VisitCode'].Type | Should be 'Code';
        }

        It 'Should have expected content' {
            $Global:TestVars['VisitCode'].Content | Should match 'This is code';
        }
    }

    Context 'Code markdown' {
        
        # Define a test document with a table
        document 'CodeMarkdown' {
            
            Code {
                This is code
            }
        }

        $outputDoc = "$outputPath\CodeMarkdown.md";
        CodeMarkdown -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match 'This is code';
        }
    }

    Context 'Code markdown with named format' {
        
        # Define a test document with a table
        document 'CodeMarkdownNamedFormat' {
            
            Code powershell {
                Get-Content
            }
        }

        $outputDoc = "$outputPath\CodeMarkdownNamedFormat.md";
        CodeMarkdownNamedFormat -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '```powershell\r\nGet-Content\r\n```';
        }
    }

    Context 'Code markdown with evaluation' {
        
        # Define a test document with a table
        document 'CodeMarkdownEval' {
            
            $a = 1; $a += 1; $a | Code powershell;
        }

        $outputDoc = "$outputPath\CodeMarkdownEval.md";
        CodeMarkdownEval -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '```powershell\r\n2\r\n```';
        }
    }
}
