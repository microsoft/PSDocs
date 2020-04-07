#
# Unit tests for the PSDocs automatic variables
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

Describe 'PSDocs variables' -Tag 'Variables' {
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Variables.doc.ps1';
    $testObject = [PSCustomObject]@{
        Name = 'TestObject'
    }

    Context 'PowerShell automatic variables' {
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $testObject
            PassThru = $True
        }
        It '$PWD' {
            $result = (Invoke-PSDocument @invokeParams -Name 'PSAutomaticVariables' | Out-String).Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries);
            $result | Where-Object -FilterScript { $_ -like "PWD=*" } | Should -Be "PWD=$PWD;";
        }
    }

    Context 'PSDocs automatic variables' {
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $testObject
            PassThru = $True
        }
        It '$Document' {
            $result = (Invoke-PSDocument @invokeParams -Name 'PSDocsAutomaticVariables' | Out-String).Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries);
            $result | Where-Object -FilterScript { $_ -like "Document.Title=*" } | Should -Be "Document.Title=001;";
            $result | Where-Object -FilterScript { $_ -like "Document.Metadata=*" } | Should -Be "Document.Metadata=002;";
        }
    }
}
