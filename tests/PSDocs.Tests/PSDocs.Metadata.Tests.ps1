#
# Unit tests for the Metadata keyword
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

$outputPath = "$temp\PSDocs.Tests\Metadata";
New-Item $outputPath -ItemType Directory -Force | Out-Null;

$dummyObject = New-Object -TypeName PSObject;

$Global:TestVars = @{ };

Describe 'PSDocs -- Metadata keyword' {
    Context 'Metadata' {

        # Define a test document with a note
        document 'MetadataVisitor' {
            
            Metadata @{
                title = 'Test'
            }
        }

        Mock -CommandName 'VisitMetadata' -ModuleName 'Markdown' -Verifiable -MockWith {
            param (
                $InputObject
            )

            $Global:TestVars['VisitMetadata'] = $InputObject;
        }

        Invoke-PSDocument -Name 'MetadataVisitor' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should process keyword' {
            Assert-MockCalled -CommandName 'VisitMetadata' -ModuleName 'Markdown' -Times 1;
        }

        It 'Should be expected type' {
            $Global:TestVars['VisitMetadata'].Type | Should be 'Document';
        }

        It 'Should have expected content' {
            $Global:TestVars['VisitMetadata'].Metadata['title'] | Should be 'Test';
        }
    }

    Context 'Metadata single entry' {
        
        # Define a test document with metadata content
        document 'MetadataSingleEntry' {
            
            Metadata ([ordered]@{
                title = 'Test'
            })
        }

        $outputDoc = "$outputPath\MetadataSingleEntry.md";
        Invoke-PSDocument -Name 'MetadataSingleEntry' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '---\r\ntitle: Test\r\n---';
        }
    }

    Context 'Metadata multiple entries' {
        
        # Define a test document with metadata content
        document 'MetadataMultipleEntry' {
            
            Metadata ([ordered]@{
                value1 = 'ABC'
                value2 = 'EFG'
            })
        }

        $outputDoc = "$outputPath\MetadataMultipleEntry.md";
        Invoke-PSDocument -Name 'MetadataMultipleEntry' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '---\r\nvalue1: ABC\r\nvalue2: EFG\r\n---';
        }
    }

    Context 'Metadata multiple blocks' {
        
        # Define a test document with metadata content
        document 'MetadataMultipleBlock' {
            
            Metadata ([ordered]@{
                value1 = 'ABC'
            })

            Section 'Test' {
                'A test section spliting metadata blocks.'
            }

            Metadata @{
                value2 = 'EFG'
            }
        }

        $outputDoc = "$outputPath\MetadataMultipleBlock.md";
        Invoke-PSDocument -Name 'MetadataMultipleBlock' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '---\r\nvalue1: ABC\r\nvalue2: EFG\r\n---';
        }
    }

    Context 'Document without Metadata block' {

        # Define a test document without metadata content
        document 'NoMetdata' {

            Section 'Test' {
                'A test section.'
            }
        }

        $outputDoc = "$outputPath\NoMetdata.md";
        Invoke-PSDocument -Name 'NoMetdata' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should not match '---\r\n';
        }
    }

    Context 'Get Metadata header' {
        
        $result = Get-PSDocumentHeader -Path $outputPath;

        It 'Should have data' {
            $result | Should not be $Null;
        }

    }
}
