#
# Build script
#

# Note:
# This script builds modules and related files.

[CmdletBinding()]
param (
    # The path to source files
    [Parameter(Mandatory = $False)]
    [String]$Path = "$PWD\src",

    # The path to stored the processed files
    [Parameter(Mandatory = $False)]
    [String]$OutputPath = "$PWD\build",
    
    # Should output paths be cleaned first
    [Parameter(Mandatory = $False)]
    [Switch]$Clean = $False,

    # The modules to build
    [Parameter(Mandatory = $False)]
    [String[]]$Module,

    [Parameter(Mandatory = $False)]
    [String[]]$IncludePackage
)

Write-Verbose -Message "[Build]`tBEGIN::";

# STEP : Add includes

# Include common library
. $PWD\scripts\common.ps1;

# STEP: Setup environment

# Setup variables
$rootPath = "$PWD";
$sourcePath = $Path;
$buildPath = $OutputPath;
$packagePath = "$rootPath\packages";
$configOut = "$buildPath";
$modulesOut = "$rootPath\artifacts\modules";

# STEP : Validate parameters

Write-Verbose -Message "[Build] -- Validating parameters";

if (!(Test-Path -Path $Path)) {
    Write-Error -Message "The specified path ($Path) does not exist.";

    return;
}

# STEP : Create output paths

Write-Verbose -Message "[Build] -- Creating output paths: $buildPath";

# Create output path
@($buildPath, $configOut, $modulesOut) | CreatePath -Clean:$Clean -Verbose:$VerbosePreference;

# STEP : Build modules

Write-Verbose -Message "[Build] -- Build modules: $buildPath";

$Module | BuildModule -Path $sourcePath -OutputPath $buildPath -Verbose:$VerbosePreference;

# STEP : Package modules

Write-Verbose -Message "[Build] -- Package modules: $modulesOut";

$Module | PackageModule -Path $buildPath -OutputPath $modulesOut -Verbose:$VerbosePreference;

# STEP : Include packages

# Write-Verbose -Message "[Build] -- Copy packages";

# if (![String]::IsNullOrEmpty($IncludePackage)) {
#     Get-ChildItem -Path $packagePath | Where-Object -FilterScript {
#         $_.Name -like $IncludePackage -or $IncludePackage -contains $_.Name
#     } | ForEach-Object -Process {
#         $targetPath = $_;

#         Write-Verbose -Message "[Build] -- Copying package: $($targetPath.Name)";

#         Copy-Item -Path $targetPath.FullName -Destination $buildPath -Recurse -Force;
#     }
# }

Write-Verbose -Message "[Build] END::";

# EOF