#
# Unit tests for the Table keyword
#

[CmdletBinding()]
param (

)

# Setup error handling
$ErrorActionPreference = 'Stop';

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

            'EOF'
        }

        $outputDoc = "$outputPath\Table.md";
        Invoke-PSDocument -Name 'TableTests' -InstanceName 'Table' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should -Match '\|LICENSE\|False\|(\n|\r){1,2}\|README.md\|False\|\r\n\r\nEOF';
        }
    }

    Context 'Table single entry markdown' {
        
        # Define a test document with a table
        document 'TableSingleEntryMarkdown' {
            
            New-Object -TypeName PSObject -Property @{ Name = 'Single' } | Table -Property Name;
        }

        $outputDoc = "$outputPath\TableSingleEntryMarkdown.md";
        Invoke-PSDocument -Name 'TableSingleEntryMarkdown' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '\|Name\|\r\n\| --- \|\r\n\|Single\|';
        }
    }
    
    Context 'Table with null' {
        
        # Define a test document with section and table
        document 'TableWithNull' {

            Section 'Windows features' {
            
                $InputObject.ResourceType.WindowsFeature | Table -Property Name,Ensure;
            }
        }

        $outputDoc = "$outputPath\TableWithNull.md";
        Invoke-PSDocument -Name 'TableWithNull' -InputObject @{ ResourceType = @{  } } -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should -Be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should -Match '(## Windows features\r\n)$';
        }
    }

    Context 'Table with multiline column' {

        $testObject = [PSCustomObject]@{
            Name = 'Test'
            Description = "This is a`r`ndescription`r`nsplit`r`nover`r`nmultiple`r`nlines."
        }

        # Define a test document with a multiple column in a table
        document 'TableWithMultilineColumn' {
            $testObject | Table;
        }

        $outputDoc = "$outputPath\TableWithMultilineColumn.md";
        Invoke-PSDocument -Name 'TableWithMultilineColumn' -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should -Be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should -Match 'This is a description split over multiple lines\.';
        }

        $option = New-PSDocumentOption @{
            'Markdown.WrapSeparator' = '<br />'
        }

        $outputDoc = "$outputPath\TableWithMultilineColumnCustom.md";
        Invoke-PSDocument -Name 'TableWithMultilineColumn' -InstanceName 'TableWithMultilineColumnCustom' -OutputPath $outputPath -Option $option;

        It 'Should use wrap separator' {
            Get-Content -Path $outputDoc -Raw | Should -Match 'This is a\<br /\>description\<br /\>split\<br /\>over\<br /\>multiple\<br /\>lines\.';
        }
    }
}
