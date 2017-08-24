#
# Test script
#

# Note:
# This script runs unit tests.

[CmdletBinding()]
param (
    # Should output paths be cleaned first
    [Parameter(Mandatory = $False)]
    [Switch]$Clean = $False
)

Write-Verbose -Message "[Test] BEGIN::";

# Include common library
. $PWD\scripts\common.ps1;

$rootPath = "$PWD";
$sourcePath = "$rootPath\src";
$reportsPath = "$rootPath\reports";
$testPath = "$rootPath\tests";

# Setup path to load modules
$Env:PSModulePath = $Env:PSModulePath + ";$rootPath\packages;$sourcePath";

if ([String]::IsNullOrEmpty($ResultsPath)) {
    $ResultsPath = "$rootPath\reports";
}

# STEP : Create output paths

Write-Verbose -Message "[Test] -- Creating output paths";

# Create output path
@($reportsPath) | CreatePath -Clean:$Clean -Verbose:$VerbosePreference;

# STEP : Run tests

$pesterModule = Get-Module -Name Pester -ListAvailable | Where-Object -FilterScript { $_.Version -like '3.4.0' };

if ($Null -eq $pesterModule) {
    Install-Module -Name Pester -RequiredVersion '3.4.0' -Force -Confirm:$False -Scope CurrentUser;
}

# Load Pester module
Import-Module -Name Pester -Verbose:$False;

@('PSDocs', 'PSDocs.Dsc') | RunTest -Path $testPath -OutputPath $reportsPath -Verbose:$VerbosePreference;

# STEP : Publish results

if (![String]::IsNullOrEmpty($Env:APPVEYOR_JOB_ID)) {
    SendAppveyorTestResult -Uri "https://ci.appveyor.com/api/testresults/nunit/$($env:APPVEYOR_JOB_ID)" -Path '.\reports' -Include '*.xml';
}

Write-Verbose -Message "[Test] END::";