#
# Test script
#

# Note:
# This script runs unit tests.

[CmdletBinding()]
param (
    # Should output paths be cleaned first
    [Parameter(Mandatory = $False)]
    [Switch]$Clean = $False,

    # Code coverage should be calculated
    [Parameter(Mandatory = $False)]
    [Switch]$CodeCoverage = $False
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

$pesterModule = Get-Module -Name Pester -ListAvailable | Where-Object -FilterScript { $_.Version -like '4.*' };

if ($Null -eq $pesterModule) {
    Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;   
     
    Install-Module -Name Pester -MinimumVersion '4.0.0' -Force -Scope CurrentUser -SkipPublisherCheck;
}

# Load Pester module
Import-Module -Name Pester -Verbose:$False;

$results = RunTest -Path $testPath -SourcePath $sourcePath -OutputPath $reportsPath -CodeCoverage:$CodeCoverage -Verbose:$VerbosePreference;

# STEP : Publish results

if (![String]::IsNullOrEmpty($Env:APPVEYOR_JOB_ID)) {
    SendAppveyorTestResult -Uri "https://ci.appveyor.com/api/testresults/nunit/$($env:APPVEYOR_JOB_ID)" -Path '.\reports' -Include '*.xml';
}

# Throw an error if pester tests failed

if ($Null -eq $results) {
    throw 'Failed to get Pester test results.';
}
elseif ($results.FailedCount -gt 0) {
    throw "$($results.FailedCount) tests failed.";
}

Write-Verbose -Message "[Test] END::";