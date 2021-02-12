#
# Unit tests for the PSDocs automatic variables
#

[CmdletBinding()]
param ()

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

# Setup tests paths
$rootPath = $PWD;
Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;
$here = (Resolve-Path $PSScriptRoot).Path;

Describe 'PSDocs variables' -Tag 'Variables' {
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Variables.Doc.ps1';
    $testObject = [PSCustomObject]@{
        Name = 'TestObject'
    }

    Context 'PowerShell automatic variables' {
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $testObject
            PassThru = $True
        }
        It 'Paths' {
            $result = (Invoke-PSDocument @invokeParams -Name 'PSAutomaticVariables' | Out-String).Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries);
            $result | Where-Object -FilterScript { $_ -like "PWD=*" } | Should -Be "PWD=$PWD;";
            $result | Where-Object -FilterScript { $_ -like "PSScriptRoot=*" } | Should -Be "PSScriptRoot=$PSScriptRoot;";
            $result | Where-Object -FilterScript { $_ -like "PSCommandPath=*" } | Should -Be "PSCommandPath=$docFilePath;";
        }
    }

    Context 'PSDocs automatic variables' {
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $testObject
            PassThru = $True
            Option = @{
                'Configuration.author' = @{ name = 'unit-tester' }
                'Configuration.enabled' = 'faLse'
            }
        }
        It '$PSDocs' {
            $result = (Invoke-PSDocument @invokeParams -Name 'PSDocsVariable' | Out-String).Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries);
            $result | Where-Object -FilterScript { $_ -like "TargetObject.Name=*" } | Should -Be 'TargetObject.Name=TestObject;';
            $result | Where-Object -FilterScript { $_ -like "Document.Metadata=*" } | Should -Be 'Document.Metadata=unit-tester;';
            $result | Where-Object -FilterScript { $_ -like "Document.Enabled=*" } | Should -BeNullOrEmpty;
        }
        It '$Document' {
            $result = (Invoke-PSDocument @invokeParams -Name 'PSDocsDocumentVariable' | Out-String).Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries);
            $result | Where-Object -FilterScript { $_ -like "Document.Title=*" } | Should -Be 'Document.Title=001;';
            $result | Where-Object -FilterScript { $_ -like "Document.Metadata=*" } | Should -Be 'Document.Metadata=002;';
        }
        It '$LocalizedData' {
            $result = (Invoke-PSDocument @invokeParams -Name 'PSDocsLocalizedDataVariable' | Out-String).Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries);
            $result | Where-Object -FilterScript { $_ -like "LocalizedData.Key1=*" } | Should -Be 'LocalizedData.Key1=Value1;';
        }
    }
}
