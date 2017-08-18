#
# Unit tests for the Table keyword
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

$outputPath = "$temp\PSDocs.Tests\Table";
New-Item $outputPath -ItemType Directory -Force | Out-Null;

$dummyObject = New-Object -TypeName PSObject;

$Global:TestVars = @{ };

Describe 'PSDocs -- Table keyword' {
    Context 'Table with a single named property' {

        # Define a test document with a table
        document 'WithSingleNamedProperty' {
            
            Get-ChildItem -Path '.\' | Table -Property 'Name'
        }

        Mock -CommandName 'VisitTable' -ModuleName 'Markdown' -Verifiable -MockWith {
            param (
                $InputObject
            )

            $Global:TestVars['VisitTable'] = $InputObject;
        }

        Invoke-PSDocument -Name 'WithSingleNamedProperty' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should process Table keyword' {
            Assert-MockCalled -CommandName 'VisitTable' -ModuleName 'Markdown' -Times 1;
        }

        It 'Should be Table object' {
            $Global:TestVars['VisitTable'].Type | Should be 'Table';
        }
    }

    Context 'Table markdown' {
        
        # Define a test document with a table
        document 'TableTests' {
            
            Get-ChildItem -Path $rootPath | Where-Object -FilterScript { 'README.md','LICENSE' -contains $_.Name } | Format-Table -Property 'Name','PSIsContainer'
        }

        $outputDoc = "$outputPath\Table.md";
        Invoke-PSDocument -Name 'TableTests' -InstanceName 'Table' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '\|LICENSE\|False\|(\n|\r){1,2}\|README.md\|False\|';
        }
    }
}
