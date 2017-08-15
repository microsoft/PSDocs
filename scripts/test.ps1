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

# Load Pester module
Import-Module -Name Pester -RequiredVersion '3.4.0' -Verbose:$False;

@('PSDocs') | RunTest -Path $testPath -OutputPath $reportsPath -Verbose:$VerbosePreference;

Write-Verbose -Message "[Test] END::";