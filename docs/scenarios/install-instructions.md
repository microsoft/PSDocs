# Install instructions

## Prerequisites

- Windows Management Framework (WMF) 5.0 or greater
- .NET Framework 4.6 or greater

## Getting the modules

- Install from [PowerShell Gallery][psg-psdocs]

```powershell
# Install base PSDocs module
Install-Module -Name 'PSDocs';
```

```powershell
# Optionally install DSC extensions module, which will install PSDocs if not already installed
Install-Module -Name 'PSDocs.Dsc';
```

- Save for offline use from PowerShell Gallery

```powershell
# Save PSDocs module, in the .\modules directory
Save-Module -Name 'PSDocs' -Path '.\modules';

# Save PSDocs.Dsc module, in the .\modules directory
Save-Module -Name 'PSDocs.Dsc' -Path '.\modules';
```

[psg-psdocs]: https://www.powershellgallery.com/packages/PSDocs
