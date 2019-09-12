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
$rootPath = $PWD;
Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;

$dummyObject = New-Object -TypeName PSObject;
$Global:TestVars = @{ };

Describe 'PSDocs -- Table keyword' -Tag Table {
    Context 'Markdown' {
        It 'With defaults' {
            document 'TableTests' {
                Get-ChildItem -Path $InputObject -File | Where-Object -FilterScript { 'README.md','LICENSE' -contains $_.Name } | Format-Table -Property 'Name','PSIsContainer'
                'EOF'
            }
            $result = TableTests -InstanceName 'Table' -InputObject $rootPath -PassThru -Option @{
                'Markdown.ColumnPadding' = 'None'
                'Markdown.UseEdgePipes' = 'Always'
            };
            $result | Should -Match '(\r|\n|\r\n)|LICENSE|False|(\r|\n|\r\n)';
            $result | Should -Match '(\r|\n|\r\n)|README.md|False|(\r|\n|\r\n)';
        }

        It 'With property expressions' {
            document 'TableWithExpression' {
                $object = [PSCustomObject]@{
                    Name = 'Dummy'
                    Property = @{
                        Value1 = 1
                        Value2 = 2
                    }
                    Value3 = 3
                }
                $object | Table -Property Name,@{ Label = 'Value1'; Alignment = 'Left'; Width = 10; Expression = { $_.Property.Value1 }},@{ Name = 'Value2'; Alignment = 'Center'; Expression = { $_.Property.Value2 }},@{ Label = 'Value3'; Expression = { $_.Value3 }; Alignment = 'Right'; };
                'EOF'
            }
            $result = TableWithExpression -PassThru;
            $result | Should -Match '(\r|\n|\r\n)---- \| :-----     \| :----: \| -----:(\r|\n|\r\n)'
            $result | Should -Match '(\r|\n|\r\n)Dummy \| 1          \| 2      \| 3(\r|\n|\r\n){2,}EOF';
        }

        It 'With single entry' {
            document 'TableSingleEntryMarkdown' {
                New-Object -TypeName PSObject -Property @{ Name = 'Single' } | Table -Property Name;
            }
            $result = TableSingleEntryMarkdown -InputObject $dummyObject -PassThru;
            $result | Should -Match '\| Name \|(\r|\n|\r\n)| -{1,} \|(\r|\n|\r\n)| Single \|';
        }

        It 'With null' {
            document 'TableWithNull' {
                Section 'Windows features' -Force {
                    $InputObject.ResourceType.WindowsFeature | Table -Property Name,Ensure;
                }
            }
            $result = TableWithNull -InputObject @{ ResourceType = @{ WindowsFeature = @() } } -PassThru;
            $result | Should -Match "`#`# Windows features(\r|\n|\r\n)$";
        }

        It 'With multiline column' {
            $testObject = [PSCustomObject]@{
                Name = 'Test'
                Description = "This is a`r`ndescription`r`nsplit`r`nover`r`nmultiple`r`nlines."
            }
            document 'TableWithMultilineColumn' {
                $InputObject | Table;
            }
            $result = TableWithMultilineColumn -InputObject $testObject -PassThru;
            $result | Should -Match 'This is a description split over multiple lines\.';

            # With separator
            $option = New-PSDocumentOption @{
                'Markdown.WrapSeparator' = '<br />'
            }
            $result = TableWithMultilineColumn -InputObject $testObject -InstanceName 'TableWithMultilineColumnCustom' -PassThru -Option $option;
            $result | Should -Match 'This is a\<br /\>description\<br /\>split\<br /\>over\<br /\>multiple\<br /\>lines\.';
        }

        It 'With null column' {
            $testObject = [PSCustomObject]@{
                Name = 'Test'
                Value = 'Value'
            }
            document 'TableWithEmptyColumn' {
                'Table1'
                $InputObject | Table -Property Name,NotValue,Value
                'Table2'
                $InputObject | Table -Property Name,NotValue
                'EOF'
            }
            $result = TableWithEmptyColumn -InputObject $testObject -InstanceName 'TableWithEmptyColumn' -PassThru;
            $result | Should -Match 'Name \| NotValue \| Value(\r|\n|\r\n)---- \| -------- \| -----(\r|\n|\r\n)Test \|          \| Value';
            $result | Should -Match 'Name \| NotValue(\r|\n|\r\n)---- \| --------(\r|\n|\r\n)Test \|(\r|\n|\r\n)';
        }
    }
}
