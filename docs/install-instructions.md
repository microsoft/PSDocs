# Install instructions

## Prerequisites

- Windows PowerShell 5.1 with .NET Framework 4.7.2+ or
- PowerShell Core 6.2 or greater on Windows, MacOS and Linux or
- PowerShell 7.0 or greater on Windows, MacOS and Linux

For a list of platforms that PowerShell 7.0 is supported on [see][get-powershell].

## Getting the module

Install from [PowerShell Gallery][module] for all users (requires permissions):

```powershell
# Install PSDocs module
Install-Module -Name 'PSDocs' -Repository PSGallery;
```

Install from [PowerShell Gallery][module] for current user only:

```powershell
# Install PSDocs module
Install-Module -Name 'PSDocs' -Repository PSGallery -Scope CurrentUser;
```

Save for offline use from PowerShell Gallery:

```powershell
# Save PSDocs module, in the .\modules directory
Save-Module -Name 'PSDocs' -Repository PSGallery -Path '.\modules';
```

> For pre-release versions the `-AllowPrerelease` switch must be added when calling `Install-Module` or `Save-Module`.
>
> To install pre-release module versions, upgrading to the latest version of _PowerShellGet_ may be required.
To do this use:
>
> `Install-Module -Name PowerShellGet -Repository PSGallery -Scope CurrentUser -Force`

## Building from source

To build this module from source run `./build.ps1`.
This build script will compile the module and documentation then output the result into `out/modules/PSDocs`.

The following PowerShell modules will be automatically downloaded if the required versions are not present:

- PlatyPS
- Pester
- PSScriptAnalyzer
- PowerShellGet
- PackageManagement
- InvokeBuild

These additional modules are only required for building PSDocs and are not required for running PSDocs.

If you are on a network that does not permit Internet access to the PowerShell Gallery,
download these modules on an alternative device that has access.
The following script can be used to download the required modules to an alternative device.
After downloading the modules copy the module directories to devices with restricted Internet access.

```powershell
# Save modules, in the .\modules directory
Save-Module -Name PlatyPS, Pester, PSScriptAnalyzer, PowerShellGet, PackageManagement, InvokeBuild -Repository PSGallery -Path '.\modules';
```

Additionally .NET Core SDK v3.1 is required.
.NET Core will not be automatically downloaded and installed.
To download and install the latest SDK see [Download .NET Core 3.1][dotnet].

## Upgrading from previous versions

If you are upgrading PSDocs from a previous version:

- Please review any breaking changes in the [change log](../CHANGELOG.md).
- See [upgrade notes](upgrade-notes.md) for helpful information.

[module]: https://www.powershellgallery.com/packages/PSDocs
[get-powershell]: https://github.com/PowerShell/PowerShell#get-powershell
[dotnet]: https://dotnet.microsoft.com/download/dotnet-core/3.1
