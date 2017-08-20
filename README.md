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

## Getting started

### 1. Prerequsits

- Windows Management Framework (WMF) 4.0 or greater
- .NET Framework 4.6 or greater

### 2. Get PSDocs

- Install from PowerShellGallery.com

```powershell
Install-Module -Name PSDocs;
```

### 3. Usage

```powershell
# Import PSDocs module
Import-Module -Name PSDocs;

# Define a sample document
document Sample {

    Section Introduction {
        # Add a comment
        "This is a sample list file from $InputObject"

        # Generate a table
        Get-ChildItem -Path $InputObject | Format-Table -Property Name,PSIsContainer
    }
}

# Call the sample document and generate markdown
Invoke-PSDocument -Name Sample -InputObject 'C:\';
```

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

## Maintainers

- [Bernie White](https://github.com/BernieWhite)

## License

This project is [licensed under the MIT License](LICENSE).