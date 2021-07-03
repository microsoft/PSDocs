# PSDocs

A PowerShell module with commands to generate markdown from objects using PowerShell syntax.

![ci-badge]

## Support

This project uses GitHub Issues to track bugs and feature requests.
Please search the existing issues before filing new issues to avoid duplicates.

- For new issues, file your bug or feature request as a new [issue].
- For help, discussion, and support questions about using this project, join or start a [discussion].

Support for this project/ product is limited to the resources listed above.

## Getting the modules

You can download and install the PSDocs module from the PowerShell Gallery.

Module     | Description | Downloads / instructions
------     | ----------- | ------------------------
PSDocs     | Generate markdown from PowerShell | [latest][psg-psdocs] / [instructions][install]

For integration modules see [related projects](#related-projects).

## Getting started

The following example shows basic PSDocs usage.
For specific use cases see [scenarios](#scenarios).

### Define a document

A document provides instructions on how PSDocs should render an object into documentation.
To define a document, create the `Document` script block saved to a file with the `.Doc.ps1` extension.

For example:

```powershell
# File: Sample.Doc.ps1

# Define a document called Sample
Document Sample {
    # Define content here
}
```

Within the document body provide one or more instructions.

For example:

```powershell
# File: Sample.Doc.ps1

# Define a document called Sample
Document Sample {

    # Add an introduction section
    Section Introduction {
        # Add a comment
        "This is a sample file list from $TargetObject"

        # Generate a table
        Get-ChildItem -Path $TargetObject | Table -Property Name,PSIsContainer
    }
}
```

### Execute a document

To execute the document use `Invoke-PSDocument`.

For example:

```powershell
Invoke-PSDocument -InputObject 'C:\';
```

An example of the output generated is available [here](docs/examples/Get-child-item-output.md).

### Scenarios

- [Azure Resource Manager template example](docs/scenarios/arm-template/arm-template.md)
- [Integration with DocFX](docs/scenarios/docfx/integration-with-docfx.md)

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

- [Invoke-PSDocument](docs/commands/PSDocs/en-US/Invoke-PSDocument.md)
- [Get-PSDocument](docs/commands/PSDocs/en-US/Get-PSDocument.md)
- [Get-PSDocumentHeader](docs/commands/PSDocs/en-US/Get-PSDocumentHeader.md)
- [New-PSDocumentOption](docs/commands/PSDocs/en-US/New-PSDocumentOption.md)

The following commands exist in the `PSDocs.Dsc` module:

- [Get-DscMofDocument](docs/commands/PSDocs.Dsc/en-US/Get-DscMofDocument.md)
- [Invoke-DscNodeDocument](docs/commands/PSDocs.Dsc/en-US/Invoke-DscNodeDocument.md)

### Concepts

The following conceptual topics exist in the `PSDocs` module:

- [Configuration](docs/concepts/PSDocs/en-US/about_PSDocs_Configuration.md)
- [Conventions](docs/concepts/PSDocs/en-US/about_PSDocs_Conventions.md)
- [Options](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md)
  - [Configuration](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#configuration)
  - [Execution.LanguageMode](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#executionlanguagemode)
  - [Markdown.ColumnPadding](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#markdowncolumnpadding)
  - [Markdown.Encoding](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#markdownencoding)
  - [Markdown.SkipEmptySections](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#markdownskipemptysections)
  - [Markdown.UseEdgePipes](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#markdownuseedgepipes)
  - [Markdown.WrapSeparator](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#markdownwrapseparator)
  - [Output.Culture](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#outputculture)
  - [Output.Path](docs/concepts/PSDocs/en-US/about_PSDocs_Options.md#outputpath)
- [Selectors](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md)
  - [AllOf](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#allof)
  - [AnyOf](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#anyof)
  - [Exists](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#exists)
  - [Equals](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#equals)
  - [Field](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#field)
  - [Greater](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#greater)
  - [GreaterOrEquals](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#greaterorequals)
  - [HasValue](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#hasvalue)
  - [In](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#in)
  - [Less](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#less)
  - [LessOrEquals](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#lessorequals)
  - [Match](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#match)
  - [Not](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#not)
  - [NotEquals](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#notequals)
  - [NotIn](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#notin)
  - [NotMatch](docs/concepts/PSDocs/en-US/about_PSDocs_Selectors.md#notmatch)
- [Variables](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md)
  - [$Culture](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#culture)
  - [$Document](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#document)
  - [$InstanceName](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#instancename)
  - [$LocalizedData](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#localizeddata)
  - [$PSDocs](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#psdocs)
  - [$TargetObject](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#targetobject)
  - [$Section](docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md#section)

## Related projects

The following projects use or integrate with PSDocs.

Name           | Description
----           | -----------
[PSDocs.Azure] | Generate documentation from Azure infrastructure as code (IaC) artifacts.
[PSDocs.Dsc]   | Extension for PSDocs to generate markdown from Desired State Configuration.

## Changes and versioning

Modules in this repository will use the [semantic versioning](http://semver.org/) model to declare breaking changes from v1.0.0.
Prior to v1.0.0, breaking changes may be introduced in minor (0.x.0) version increments.
For a list of module changes please see the [change log](CHANGELOG.md).

> Pre-release module versions are created on major commits and can be installed from the PowerShell Gallery.
> Pre-release versions should be considered experimental.
> Modules and change log details for pre-releases will be removed as standard releases are made available.

## Contributing

This project welcomes contributions and suggestions.
If you are ready to contribute, please visit the [contribution guide](CONTRIBUTING.md).

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Maintainers

- [Bernie White](https://github.com/BernieWhite)

## License

This project is [licensed under the MIT License](LICENSE).

[install]: docs/install-instructions.md
[issue]: https://github.com/Microsoft/PSDocs/issues
[discussion]: https://github.com/Microsoft/PSDocs/discussions
[ci-badge]: https://bewhite.visualstudio.com/PSDocs/_apis/build/status/PSDocs-CI?branchName=main
[psg-psdocs]: https://www.powershellgallery.com/packages/PSDocs
[psg-psdocs-version-badge]: https://img.shields.io/powershellgallery/v/PSDocs.svg
[psg-psdocs-installs-badge]: https://img.shields.io/powershellgallery/dt/PSDocs.svg
[PSDocs.Dsc]: https://www.powershellgallery.com/packages/PSDocs.Dsc
[PSDocs.Azure]: https://azure.github.io/PSDocs.Azure/
