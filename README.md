# PSDocs

A PowerShell module with commands to generate markdown from objects using PowerShell syntax.

![ci-badge]

## Disclaimer

This project is to be considered a **proof-of-concept** and **not a supported product**.

If you have any problems please check our GitHub [issues](https://github.com/BernieWhite/PSDocs/issues) page. If you do not see your problem captured, please file a new issue and follow the provided template.

## Getting the modules

You can download and install these PowerShell modules from the PowerShell Gallery.

| Module     | Description | Downloads / instructions |
| ------     | ----------- | ------------------------ |
| PSDocs     | Generate markdown from PowerShell | [latest][psg-psdocs] / [instructions][install] |
| PSDocs.Dsc | Extension for PSDocs to generate markdown from Desired State Configuration | [latest][psg-psdocsdsc] / [instructions][install] |

## Getting started

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

# Call the document definition as a function to generate markdown from an object
Sample -InputObject 'C:\';
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

### Scenarios

- [Azure Resource Manager template example](docs/scenarios/arm-template/arm-template.md)
- [Integration with DocFX](docs/scenarios/integration-with-docfx.md)

## Language reference

PSDocs extends PowerShell with domain specific language (DSL) keywords and cmdlets.

### Keywords

The following language keywords are used by the `PSDocs` module:

- [Document](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#document) - Defines a named documentation block
- [Section](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#section) - Creates a named section
- [Title](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#title) - Sets the document title
- [Code](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#code) - Inserts a block of code
- [BlockQuote](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#blockquote) - Inserts a block quote
- [Note](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#note) - Inserts a note using DocFx formatted markdown (DFM)
- [Warning](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#warning) - Inserts a warning using DocFx formatted markdown (DFM)
- [Metadata](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#metadata) - Inserts a yaml header
- [Table](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#table) - Inserts a table from pipeline objects
- [Include](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#include) - Insert content from an external file

### Commands

The following commands exist in the `PSDocs` module:

- [Invoke-PSDocument](/docs/commands/PSDocs/en-US/Invoke-PSDocument.md)
- [Get-PSDocumentHeader](/docs/commands/PSDocs/en-US/Get-PSDocumentHeader.md)
- [New-PSDocumentOption](/docs/commands/PSDocs/en-US/New-PSDocumentOption.md)

The following commands exist in the `PSDocs.Dsc` module:

- [Get-DscMofDocument](/docs/commands/PSDocs.Dsc/en-US/Get-DscMofDocument.md)
- [Invoke-DscNodeDocument](/docs/commands/PSDocs.Dsc/en-US/Invoke-DscNodeDocument.md)

### Concepts

The following conceptual topics exist in the `PSDocs` module:

- [Options](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md)
  - [Markdown.WrapSeparator](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#wrap-separator)
  - [Markdown.Encoding](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#encoding)
  - [Markdown.SkipEmptySections](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#skip-empty-sections)
  - [Markdown.ColumnPadding](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#column-padding)
  - [Markdown.UseEdgePipes](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#use-edge-pipes)
  - [Execution.LanguageMode](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#language-mode)
- [Variables](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md)
  - [$Culture](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#culture)
  - [$Document](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#document)
  - [$InstanceName](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#instancename)
  - [$InputObject](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#inputobject)
  - [$Section](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#section)

## Changes and versioning

Modules in this repository will use the [semantic versioning](http://semver.org/) model to declare breaking changes from v1.0.0. Prior to v1.0.0, breaking changes may be introduced in minor (0.x.0) version increments. For a list of module changes please see the [change log](CHANGELOG.md).

## Maintainers

- [Bernie White](https://github.com/BernieWhite)

## License

This project is [licensed under the MIT License](LICENSE).

[install]: docs/scenarios/install-instructions.md
[ci-badge]: https://bewhite.visualstudio.com/PSDocs/_apis/build/status/PSDocs-CI?branchName=master
[psg-psdocs]: https://www.powershellgallery.com/packages/PSDocs
[psg-psdocs-version-badge]: https://img.shields.io/powershellgallery/v/PSDocs.svg
[psg-psdocs-installs-badge]: https://img.shields.io/powershellgallery/dt/PSDocs.svg
[psg-psdocsdsc]: https://www.powershellgallery.com/packages/PSDocs.Dsc
[psg-psdocsdsc-version-badge]: https://img.shields.io/powershellgallery/v/PSDocs.Dsc.svg
[psg-psdocsdsc-installs-badge]: https://img.shields.io/powershellgallery/dt/PSDocs.Dsc.svg
