#
# CI script for integration with Azure DevOps
#

[CmdletBinding()]
param (
    [Parameter(Mandatory = $True)]
    [String]$File,

    [Parameter(Mandatory = $False)]
    [String]$Task,

    [Parameter(Mandatory = $False)]
    [String]$ModuleVersion,

    [Parameter(Mandatory = $False)]
    [AllowNull()]
    [String]$ReleaseVersion,

    [Parameter(Mandatory = $False)]
    [String]$Configuration = 'Debug',

    [Parameter(Mandatory = $False)]
    [String]$NuGetApiKey,

    [Parameter(Mandatory = $False)]
    [Switch]$CodeCoverage = $False,

    [Parameter(Mandatory = $False)]
    [Switch]$Benchmark = $False,

    [Parameter(Mandatory = $False)]
    [String]$ArtifactPath = (Join-Path -Path $PWD -ChildPath out/modules)
)

if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction SilentlyContinue)) {
    Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
}

if ($Null -eq (Get-InstalledModule -Name PowerShellGet -MinimumVersion 2.0.3 -ErrorAction SilentlyContinue)) {
    Install-Module PowerShellGet -MinimumVersion 2.0.3 -Scope CurrentUser -Force;
    Remove-Module -Name PowerShellGet -ErrorAction SilentlyContinue;
    Import-Module -Name PowerShellGet -MinimumVersion 2.0.3;
}

if ($Null -eq (Get-Module -Name InvokeBuild -ListAvailable -ErrorAction SilentlyContinue | Where-Object -FilterScript { $_.Version -like '5.*' })) {
    Install-Module InvokeBuild -MinimumVersion 5.4.0 -Scope CurrentUser -Force;
}

Invoke-Build @PSBoundParameters
