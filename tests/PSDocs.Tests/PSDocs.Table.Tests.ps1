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
$here = (Resolve-Path $PSScriptRoot).Path;

Describe 'PSDocs -- Table keyword' -Tag Table {
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Keyword.doc.ps1';

    Context 'Markdown' {
        $dummyObject = New-Object -TypeName PSObject;
        $invokeParams = @{
            Path = $docFilePath
            PassThru = $True
        }
        It 'With defaults' {
            $result = Invoke-PSDocument @invokeParams -InputObject $rootPath -InstanceName Table -Name 'TableTests' -Option @{
                'Markdown.ColumnPadding' = 'None'
                'Markdown.UseEdgePipes' = 'Always'
            };
            $result | Should -Match '(\r|\n|\r\n)|LICENSE|False|(\r|\n|\r\n)';
            $result | Should -Match '(\r|\n|\r\n)|README.md|False|(\r|\n|\r\n)';
        }

        It 'With property expressions' {
            $result = Invoke-PSDocument @invokeParams -Name 'TableWithExpression';
            $result | Should -Match '(\r|\n|\r\n)---- \| :-----     \| :----: \| -----:(\r|\n|\r\n)'
            $result | Should -Match '(\r|\n|\r\n)Dummy \| 1          \| 2      \| 3(\r|\n|\r\n){2,}EOF';
        }

        It 'With single entry' {
            $result = Invoke-PSDocument @invokeParams -Name 'TableSingleEntryMarkdown';
            $result | Should -Match '\| Name \|(\r|\n|\r\n)| -{1,} \|(\r|\n|\r\n)| Single \|';
        }

        It 'With null' {
            $result = Invoke-PSDocument @invokeParams -Name 'TableWithNull' -InputObject @{ ResourceType = @{ WindowsFeature = @() } };
            $result | Should -Match "`#`# Windows features(\r|\n|\r\n)$";
        }

        It 'With multiline column' {
            $testObject = [PSCustomObject]@{
                Name = 'Test'
                Description = "This is a`r`ndescription`r`nsplit`r`nover`r`nmultiple`r`nlines."
            }
            
            $result = TableWithMultilineColumn -InputObject $testObject -PassThru;
            $result | Should -Match 'This is a description split over multiple lines\.';

            # With separator
            $option = @{
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
            
            $result = TableWithEmptyColumn -InputObject $testObject -InstanceName 'TableWithEmptyColumn' -PassThru;
            $result | Should -Match 'Name \| NotValue \| Value(\r|\n|\r\n)---- \| -------- \| -----(\r|\n|\r\n)Test \|          \| Value';
            $result | Should -Match 'Name \| NotValue(\r|\n|\r\n)---- \| --------(\r|\n|\r\n)Test \|(\r|\n|\r\n)';
        }
    }
}
