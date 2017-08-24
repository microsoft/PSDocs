# PSDocs
A PowerShell module with commands to generate markdown from objects using PowerShell syntax.

| AppVeyor (Windows) | Codecov (Windows) |
| --- | --- |
| [![av-image][]][av-site] | [![cc-image][]][cc-site] |

[av-image]: https://ci.appveyor.com/api/projects/status/pl7tu7ktue388n7s
[av-site]: https://ci.appveyor.com/project/BernieWhite/psdocs
[cc-image]: https://codecov.io/gh/BernieWhite/PSDocs/branch/master/graph/badge.svg
[cc-site]: https://codecov.io/gh/BernieWhite/PSDocs

## Disclaimer
This project is to be considered a **proof-of-concept** and **not a supported Microsoft product**.

## Modules
The following modules are included in this repository.

| Module     | Description | Latest version |
| ------     | ----------- | -------------- |
| PSDocs     | Generate markdown from PowerShell | [v0.2.0][psg-psdocs] |
| PSDocs.Dsc | Extension for PSDocs to generate markdown from Desired State Configuration | [v0.2.0][psg-psdocsdsc] |

[psg-psdocs]: https://www.powershellgallery.com/packages/PSDocs
[psg-psdocsdsc]: https://www.powershellgallery.com/packages/PSDocs.Dsc

## Getting started

### 1. Prerequsits

- Windows Management Framework (WMF) 5.0 or greater
- .NET Framework 4.6 or greater

### 2. Get PSDocs

- Install from PowerShellGallery.com

```powershell
# Install base PSDocs module
Install-Module -Name 'PSDocs';
```

```powershell
# Optionally install DSC extensions module, which will install PSDocs if not already installed
Install-Module -Name 'PSDocs.Dsc';
```

### 3. Usage

```powershell
# Import PSDocs module
Import-Module -Name PSDocs;

# Define a sample document
document Sample {

    Section Introduction {
        # Add a comment
        "This is a sample file list from $InputObject"

        # Generate a table
        Get-ChildItem -Path $InputObject | Table -Property Name,PSIsContainer
    }
}

# Call the sample document and generate markdown
Invoke-PSDocument -Name Sample -InputObject 'C:\';
```

For an example of the output generated see [Get-ChildItemExample](/docs/examples/Get-child-item-output.md)

## Language reference

### Keywords

- [Document](/docs/keywords/Document.md)
- [Section](/docs/keywords/Section.md)
- [Code](/docs/keywords/Code.md)
- [Note](/docs/keywords/Note.md)
- [Warning](/docs/keywords/Warning.md)
- [Yaml](/docs/keywords/Yaml.md)
- [Table](/docs/keywords/Table.md)

### Commands

- [Invoke-PSDocument](/docs/commands/Invoke-PSDocument.md)
- [Invoke-DscNodeDocument](/docs/commands/Invoke-DscNodeDocument.md)

## Maintainers

- [Bernie White](https://github.com/BernieWhite)

## License

This project is [licensed under the MIT License](LICENSE).