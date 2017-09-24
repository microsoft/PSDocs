#
# Unit tests for the Yaml keyword
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

$outputPath = "$temp\PSDocs.Tests\Yaml";
New-Item $outputPath -ItemType Directory -Force | Out-Null;

$dummyObject = New-Object -TypeName PSObject;

$Global:TestVars = @{ };

Describe 'PSDocs -- Yaml keyword' {
    Context 'Yaml' {

        # Define a test document with a note
        document 'YamlVisitor' {
            
            Yaml @{
                title = 'Test'
            }
        }

        Mock -CommandName 'VisitYaml' -ModuleName 'Markdown' -Verifiable -MockWith {
            param (
                $InputObject
            )

            $Global:TestVars['VisitYaml'] = $InputObject;
        }

        Invoke-PSDocument -Name 'YamlVisitor' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should process keyword' {
            Assert-MockCalled -CommandName 'VisitYaml' -ModuleName 'Markdown' -Times 1;
        }

        It 'Should be expected type' {
            $Global:TestVars['VisitYaml'].Type | Should be 'Document';
        }

        It 'Should have expected content' {
            $Global:TestVars['VisitYaml'].Metadata['title'] | Should be 'Test';
        }
    }

    Context 'Yaml single entry' {
        
        # Define a test document with yaml content
        document 'YamlSingleEntry' {
            
            Yaml ([ordered]@{
                title = 'Test'
            })
        }

        $outputDoc = "$outputPath\YamlSingleEntry.md";
        Invoke-PSDocument -Name 'YamlSingleEntry' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '---\r\ntitle: Test\r\n---';
        }
    }

    Context 'Yaml multiple entries' {
        
        # Define a test document with yaml content
        document 'YamlMultipleEntry' {
            
            Yaml ([ordered]@{
                value1 = 'ABC'
                value2 = 'EFG'
            })
        }

        $outputDoc = "$outputPath\YamlMultipleEntry.md";
        Invoke-PSDocument -Name 'YamlMultipleEntry' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '---\r\nvalue1: ABC\r\nvalue2: EFG\r\n---';
        }
    }

    Context 'Yaml multiple blocks' {
        
        # Define a test document with yaml content
        document 'YamlMultipleBlock' {
            
            Yaml ([ordered]@{
                value1 = 'ABC'
            })

            Section 'Test' {
                'A test section spliting yaml blocks.'
            }

            Yaml @{
                value2 = 'EFG'
            }
        }

        $outputDoc = "$outputPath\YamlMultipleBlock.md";
        Invoke-PSDocument -Name 'YamlMultipleBlock' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '---\r\nvalue1: ABC\r\nvalue2: EFG\r\n---';
        }
    }

    Context 'Get Yaml header' {
        
        $result = Get-PSDocumentHeader -Path $outputPath;

        It 'Should have data' {
            $result | Should not be $Null;
        }

    }
}
