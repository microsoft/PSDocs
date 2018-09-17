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
$rootPath = Join-Path -Path $PSScriptRoot -ChildPath ../.. -Resolve;

Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs") -Force;

$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSDocs.Tests/Table;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction SilentlyContinue;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

$dummyObject = New-Object -TypeName PSObject;

Describe 'PSDocs -- Table keyword' -Tag Table {

    Context 'Table markdown' {

        # Define a test document with a table
        document 'TableTests' {

            Get-ChildItem -Path $rootPath | Where-Object -FilterScript { 'README.md','LICENSE' -contains $_.Name } | Format-Table -Property 'Name','PSIsContainer'

            'EOF'
        }

        $outputDoc = "$outputPath\Table.md";
        TableTests -InstanceName 'Table' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            $content = Get-Content -Path $outputDoc;
            $content | Should -Contain '|LICENSE|False|';
            $content | Should -Contain '|README.md|False|';
        }
    }

    Context 'Table with property expression' {

        # Define table with property expressions
        document 'TableWithExpression' {
            
            $object = [PSCustomObject]@{
                Name = 'Dummy'
                Property = @{
                    Value1 = 1
                    Value2 = 2
                }
            }

            $object | Table -Property Name,@{ Name = 'Value1'; Expression = { $_.Property.Value1 }},@{ Name = 'Value2'; Expression = { $_.Property.Value2 }};

            'EOF'
        }

        $outputDoc = "$outputPath\TableWithExpression.md";
        TableWithExpression -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            $outputDoc | Should -FileContentMatchMultiline '\|Dummy\|1\|2\|\r\n\r\nEOF';
        }
    }

    Context 'Table single entry markdown' {
        
        # Define a test document with a table
        document 'TableSingleEntryMarkdown' {
            
            New-Object -TypeName PSObject -Property @{ Name = 'Single' } | Table -Property Name;
        }

        $outputDoc = "$outputPath\TableSingleEntryMarkdown.md";
        TableSingleEntryMarkdown -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            $outputDoc | Should -FileContentMatchMultiline '\|Name\|\r\n\| --- \|\r\n\|Single\|';
        }
    }
    
    Context 'Table with null' {
        
        # Define a test document with section and table
        document 'TableWithNull' {
            Section 'Windows features' -Force {
                $InputObject.ResourceType.WindowsFeature | Table -Property Name,Ensure;
            }
        }

        $outputDoc = "$outputPath\TableWithNull.md";
        TableWithNull -InputObject @{ ResourceType = @{ WindowsFeature = @() } } -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should -Be $True;
        }

        It 'Should match expected format' {
            $outputDoc | Should -FileContentMatchMultiline '(## Windows features\r\n)$';
        }
    }

    Context 'Table with multiline column' {

        $testObject = [PSCustomObject]@{
            Name = 'Test'
            Description = "This is a`r`ndescription`r`nsplit`r`nover`r`nmultiple`r`nlines."
        }

        # Define a test document with a multiple column in a table
        document 'TableWithMultilineColumn' {
            $InputObject | Table;
        }

        $outputDoc = "$outputPath\TableWithMultilineColumn.md";
        TableWithMultilineColumn -InputObject $testObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should -Be $True;
        }

        It 'Should match expected format' {
            $outputDoc | Should -FileContentMatch 'This is a description split over multiple lines\.';
        }

        $option = New-PSDocumentOption @{
            'Markdown.WrapSeparator' = '<br />'
        }

        $outputDoc = "$outputPath\TableWithMultilineColumnCustom.md";
        TableWithMultilineColumn -InputObject $testObject -InstanceName 'TableWithMultilineColumnCustom' -OutputPath $outputPath -Option $option;

        It 'Should use wrap separator' {
            $outputDoc | Should -FileContentMatch 'This is a\<br /\>description\<br /\>split\<br /\>over\<br /\>multiple\<br /\>lines\.';
        }
    }
}
