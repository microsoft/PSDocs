# PSDocs

A PowerShell module with commands to generate markdown from objects using PowerShell syntax.

| AppVeyor (Windows) | Codecov (Windows) |
| --- | --- |
| [![av-image][]][av-site] | [![cc-image][]][cc-site] |

[av-image]: https://ci.appveyor.com/api/projects/status/pl7tu7ktue388n7s
[av-site]: https://ci.appveyor.com/project/BernieWhite/PSDocs
[cc-image]: https://codecov.io/gh/BernieWhite/PSDocs/branch/master/graph/badge.svg
[cc-site]: https://codecov.io/gh/BernieWhite/PSDocs

## Disclaimer

This project is to be considered a **proof-of-concept** and **not a supported Microsoft product**.

## Modules

The following modules are included in this repository.

| Module     | Description | Latest version |
| ------     | ----------- | -------------- |
| PSDocs     | Generate markdown from PowerShell | [v0.3.0][psg-psdocs] |
| PSDocs.Dsc | Extension for PSDocs to generate markdown from Desired State Configuration | [v0.3.0][psg-psdocsdsc] |

[psg-psdocs]: https://www.powershellgallery.com/packages/PSDocs
[psg-psdocsdsc]: https://www.powershellgallery.com/packages/PSDocs.Dsc

## Getting started

### Prerequsits

- Windows Management Framework (WMF) 5.0 or greater
- .NET Framework 4.6 or greater

### Getting the modules

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

### Generate a document from a directory listing

```powershell
# Import PSDocs module
Import-Module -Name PSDocs;

# Define a document called Sample
Document Sample {

    # Add an introduction section
    Section Introduction {
        # Add a comment
        "This is a sample file list from $InputObject"

        # Generate a table
        Get-ChildItem -Path $InputObject | Table -Property Name,PSIsContainer
    }
}

# Call the document definition and generate markdown from an object
Invoke-PSDocument -Name 'Sample' -InputObject 'C:\';
```

An example of the output generated is available [here](/docs/examples/Get-child-item-output.md).

### Generate documentation from Desired State Configuration

```powershell
# Import PSDocs.Dsc module
Import-Module -Name PSDocs.Dsc;

# Define a document called Sample
Document 'Sample' {

    # Add an 'Installed features' section in the document
    Section 'Installed features' {
        # Add a comment
        'The following Windows features have been installed.'

        # Generate a table of Windows Features
        $InputObject.ResourceType.WindowsFeature | Table -Property Name,Ensure
    }
}

# Call the document definition and generate markdown for each .mof file in the .\nodes directory
Invoke-DscNodeDocument -DocumentName 'Sample' -Path '.\nodes' -OutputPath '.\docs';
```

## Language reference

PSDocs extends PowerShell with domain specific lanagage (DSL) keywords and cmdlets.

### Keywords

The following language keywords are used by the `PSDocs` module:

- [Document](/docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#Document) - Defines a named documentation block
- [Section](/docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#Section) - Creates a named section
- [Title](/docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#Title) - Sets the document title
- [Code](/docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#Code) - Inserts a block of code
- [Note](/docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#Note) - Inserts a note using DocFx formatted markdown (DFM)
- [Warning](/docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#Warning) - Inserts a warnding usinf DocFx formatted markdown (DFM)
- [Yaml](/docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#Yaml) - Inserts a YAML header
- [Table](/docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#Table) - Inserts a table from pipeline objects

### Commands

The following commands exist in the `PSDocs` module:

- [Invoke-PSDocument](/docs/commands/PSDocs/en-US/Invoke-PSDocument.md)
- [Get-PSDocumentHeader](/docs/commands/PSDocs/en-US/Get-PSDocumentHeader.md)
- [Import-PSDocumentTemplate](/docs/commands/PSDocs/en-US/Import-PSDocumentTemplate.md)

The following commands exist in the `PSDocs.Dsc` module:

- [Get-DscMofDocument](/docs/commands/PSDocs.Dsc/en-US/Get-DscMofDocument.md)
- [Invoke-DscNodeDocument](/docs/commands/PSDocs.Dsc/en-US/Invoke-DscNodeDocument.md)

## Changes and versioning

Modules in this repository will use the [semantic versioning](http://semver.org/) model to delcare breaking changes from v1.0.0. Prior to v1.0.0, breaking changes may be introduced in minor (0.x.0) version increments. For a list of module changes please see the [change log](CHANGELOG.md).

## Maintainers

- [Bernie White](https://github.com/BernieWhite)

## License

This project is [licensed under the MIT License](LICENSE).

[psg-psdocs]: https://www.powershellgallery.com/packages/PSDocs