#
# Unit tests for the Section keyword
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

$outputPath = "$temp\PSDocs.Tests";
New-Item $outputPath -ItemType Directory -Force | Out-Null;

$dummyObject = New-Object -TypeName PSObject;

document 'SectionBlockTests' {
    Section 'Test' {

    }
}

Describe 'PSDocs -- Section keyword' {
    Context 'Section block' {

        Mock -CommandName 'VisitSection' -ModuleName 'PSDocs' -Verifiable -MockWith {
            param (
                $Input
            )
        }

        $result = Invoke-PSDocument -Name 'SectionBlockTests' -InstanceName 'SectionBlock' -InputObject $dummyObject -OutputPath $outputPath;

        # It 'Section block processed successfully' {
        #     $result | Should not be $Null;
        # }

        Assert-MockCalled -CommandName 'VisitSection' -ModuleName 'PSDocs' -Times 1;
    }
}
