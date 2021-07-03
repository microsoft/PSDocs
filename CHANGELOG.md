# Change Log

## Unreleased

- Engine features:
  - Added support for reading objects from file. [#132](https://github.com/Microsoft/PSDocs/issues/132) [#131](https://github.com/Microsoft/PSDocs/issues/131)
  - Added support for conditionally processing documents based on target object. [#133](https://github.com/Microsoft/PSDocs/issues/113)
    - **Breaking change**: Documents that do not set a body are skipped.
    - Conditionally process target objects with script block or selector based conditions.
    - Script block based conditions are PowerShell code that can be added to `Document` blocks with `-If`.
    - Selector block based conditions are YAML filters that can be added to `Document` blocks with `-With`.
    - Added options for configuring processing of input.
    - See [about_PSDocs_Options] for more details.
- General improvements:
  - Added schema for PSDocs configuration options within `ps-docs.yaml`. [#113](https://github.com/Microsoft/PSDocs/issues/113)

See [upgrade notes](docs/upgrade-notes.md) for helpful information when upgrading from previous versions.

## v0.9.0-B2107002 (pre-release)

What's changed since pre-release v0.9.0-B2106004:

- Engineering:
  - Migrated PSDocs to Microsoft GitHub organization. [#145](https://github.com/microsoft/PSDocs/issues/145)
  - Bump YamlDotNet dependency to v11.2.1. [#168](https://github.com/microsoft/PSDocs/pull/168)

## v0.9.0-B2106004 (pre-release)

What's changed since pre-release v0.9.0-B2102002:

- Engineering:
  - Bump YamlDotNet dependency to v11.2.0. [#165](https://github.com/Microsoft/PSDocs/pull/165)

## v0.9.0-B2102002 (pre-release)

What's changed since v0.8.0:

- General improvements:
  - Added support for document data and metadata in `end` convention blocks. [#148](https://github.com/Microsoft/PSDocs/issues/148)

## v0.8.0

What's changed since v0.7.0:

- Engine features:
  - Added support for running custom actions using conventions. [#18](https://github.com/Microsoft/PSDocs/issues/18)
  [#120](https://github.com/Microsoft/PSDocs/issues/120)
    - Conventions provide `Begin`, `Process` and `End` blocks to hook into the document pipeline.
    - Name or change the output path of documents in `Begin` and `Process` blocks.
    - Generate table of contents (TOC) and perform publishing actions in `End` blocks.
    - See [about_PSDocs_Conventions] for more details.
  - Added support for custom configuration key values. [#121](https://github.com/Microsoft/PSDocs/issues/121)
    - See [about_PSDocs_Configuration] for more details.
- General improvements:
  - Improve handling when an empty document title is set. [#122](https://github.com/Microsoft/PSDocs/issues/122)
  - Added `-Replace` parameter to `Include` keyword to replace tokens in included file. [#134](https://github.com/Microsoft/PSDocs/issues/134)
    - A hashtable of replacement tokens can be specified to replace contents within original file.
    - See [about_PSDocs_Keywords] for more details.
- Bug fixes:
  - Fixed boolean string conversion with the `GetBoolOrDefault` configuration helper. [#140](https://github.com/Microsoft/PSDocs/issues/140)
  - Fixed use of error action preference with `Include` keyword. [#127](https://github.com/Microsoft/PSDocs/issues/127)

What's changed since pre-release v0.8.0-B2102012:

- No additional changes.

## v0.8.0-B2102012 (pre-release)

What's changed since pre-release v0.8.0-B2101011:

- Engine features:
  - Added support for running custom actions using conventions. [#18](https://github.com/Microsoft/PSDocs/issues/18)
  [#120](https://github.com/Microsoft/PSDocs/issues/120)
    - Conventions provide `Begin`, `Process` and `End` blocks to hook into the document pipeline.
    - Name or change the output path of documents in `Begin` and `Process` blocks.
    - Generate table of contents (TOC) and perform publishing actions in `End` blocks.
    - See [about_PSDocs_Conventions] for more details.
- Bug fixes:
  - Fixed boolean string conversion with the `GetBoolOrDefault` configuration helper. [#140](https://github.com/Microsoft/PSDocs/issues/140)

## v0.8.0-B2101011 (pre-release)

What's changed since pre-release v0.8.0-B2101006:

- General improvements:
  - Added `-Replace` parameter to `Include` keyword to replace tokens in included file. [#134](https://github.com/Microsoft/PSDocs/issues/134)
    - A hashtable of replacement tokens can be specified to replace contents within original file.
    - See [about_PSDocs_Keywords] for more details.

## v0.8.0-B2101006 (pre-release)

What's changed since v0.7.0:

- Engine features:
  - Added support for custom configuration key values. [#121](https://github.com/Microsoft/PSDocs/issues/121)
    - See [about_PSDocs_Configuration] for more details.
- General improvements:
  - Improve handling when an empty document title is set. [#122](https://github.com/Microsoft/PSDocs/issues/122)
- Bug fixes:
  - Fixed use of error action preference with `Include` keyword. [#127](https://github.com/Microsoft/PSDocs/issues/127)

## v0.7.0

What's changed since v0.6.3:

- Engine features:
  - Added support for MacOS and Linux. [#59](https://github.com/Microsoft/PSDocs/issues/59)
  - Added support for using document definitions from modules. [#81](https://github.com/Microsoft/PSDocs/issues/81)
  - Added support for localized strings using the `$LocalizedData` variable. [#91](https://github.com/Microsoft/PSDocs/issues/91)
  - Automatically serialize `Code` objects to JSON and YAML. [#93](https://github.com/Microsoft/PSDocs/issues/93)
    - Use the `json`, `yaml`, or `yml` info strings to automatically serialize custom objects.
    - See [about_PSDocs_Keywords] for more details.
- General improvements:
  - Added configuration for setting output options. [#105](https://github.com/Microsoft/PSDocs/issues/105)
  - Added parameter alias `-MarkdownEncoding` on `New-PSDocumentOption` for `-Encoding`. [#106](https://github.com/Microsoft/PSDocs/issues/106)
  - Default the info string to `powershell` for `Code` script blocks. [#92](https://github.com/Microsoft/PSDocs/issues/92)
    - See [about_PSDocs_Keywords] for more details.
- Deprecations and removals:
  - Added [upgrade notes](docs/upgrade-notes.md) for migration from v0.6.x to v0.7.0.
  - **Breaking change**: Removed support for inline document blocks.
    - Use `Invoke-PSDocument` with `.Doc.ps1` files instead.
    - Helper functions within the script scope must be flagged with `global` scope.
  - **Breaking change**: Removed script block usage of `Note` and `Warning`.
    - Script block support was previously deprecated in v0.6.0.
    - Use pipeline instead.
  - **Breaking change**: Removed support for `-When` section parameter.
    - `-When` was previously replaced with `-If` in v0.6.0.
- Engineering:
  - Bump YamlDotNet dependency to v8.1.2.
- Bug fixes:
  - Fixed inconsistencies with default options file name. [#103](https://github.com/Microsoft/PSDocs/issues/103)
  - Fixed line break after block quote. [#104](https://github.com/Microsoft/PSDocs/issues/104)

See [upgrade notes](docs/upgrade-notes.md) for helpful information when upgrading from previous versions.

What's changed since pre-release v0.7.0-B2101015:

- No additional changes.

## v0.7.0-B2101015 (pre-release)

What's changed since pre-release v0.7.0-B2008035:

- Engine features:
  - Added support for localized strings using the `$LocalizedData` variable. [#91](https://github.com/Microsoft/PSDocs/issues/91)
  - Automatically serialize `Code` objects to JSON and YAML. [#93](https://github.com/Microsoft/PSDocs/issues/93)
    - Use the `json`, `yaml`, or `yml` info strings to automatically serialize custom objects.
    - See [about_PSDocs_Keywords] for more details.
- General improvements:
  - Added configuration for setting output options. [#105](https://github.com/Microsoft/PSDocs/issues/105)
  - Added parameter alias `-MarkdownEncoding` on `New-PSDocumentOption` for `-Encoding`. [#106](https://github.com/Microsoft/PSDocs/issues/106)
  - Default the info string to `powershell` for `Code` script blocks. [#92](https://github.com/Microsoft/PSDocs/issues/92)
    - See [about_PSDocs_Keywords] for more details.
- Bug fixes:
  - Fixed inconsistencies with default options file name. [#103](https://github.com/Microsoft/PSDocs/issues/103)
  - Fixed line break after block quote. [#104](https://github.com/Microsoft/PSDocs/issues/104)

## v0.7.0-B2008035 (pre-release)

What's changed since pre-release v0.7.0-B2008022:

- Engine features:
  - Added support for using document definitions from modules. [#81](https://github.com/Microsoft/PSDocs/issues/81)

## v0.7.0-B2008022 (pre-release)

What's changed since v0.6.3:

- Engine features:
  - Added support for MacOS and Linux. [#59](https://github.com/Microsoft/PSDocs/issues/59)
- Deprecations and removals:
  - Added [upgrade notes](docs/upgrade-notes.md) for migration from v0.6.x to v0.7.0.
  - **Breaking change**: Removed support for inline document blocks.
    - Use `Invoke-PSDocument` with `.Doc.ps1` files instead.
    - Helper functions within the script scope must be flagged with `global` scope.
  - **Breaking change**: Removed script block usage of `Note` and `Warning`.
    - Script block support was previously deprecated in v0.6.0.
    - Use pipeline instead.
  - **Breaking change**: Removed support for `-When` section parameter.
    - `-When` was previously replaced with `-If` in v0.6.0.
- Engineering:
  - Bump YamlDotNet dependency to v8.1.2.

See [upgrade notes](docs/upgrade-notes.md) for helpful information when upgrading from previous versions.

## v0.6.3

What's changed since v0.6.2:

- Bug fixes:
  - Fix concatenation of multiple lines in Code section. [#69](https://github.com/Microsoft/PSDocs/issues/69)

## v0.6.2

What's changed since v0.6.1:

- Bug fixes:
  - Fixed PositionMessage cannot be found on this object. [#63](https://github.com/Microsoft/PSDocs/issues/63)
  - Fixed handling of null metadata hashtable. [#60](https://github.com/Microsoft/PSDocs/issues/60)

## v0.6.1

What's changed since v0.6.0:

- Bug fixes:
  - Fixed null reference for table columns with undefined properties. [#53](https://github.com/Microsoft/PSDocs/issues/53)

## v0.6.0

What's changed since v0.5.0:

- Engine features:
  - Added `BlockQuote` keyword to generate block quotes in addition to existing `Note` and `Warning` keywords which are specific to DocFX.
    - See [about_PSDocs_Keywords](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#blockquote) help topic for details.
  - Added `-Culture` parameter to `Invoke-PSDocument`, which allows generation of multiple localized output files.
  - Added `Include` keyword to insert content from an external file.
    - Use the `-UseCulture` switch of `Include` to insert content from a culture specific external file.
    - See [about_PSDocs_Keywords](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#include) help topic for details.
- General improvements:
  - Added new contextual help topic to provide details on automatic variables exposed for PSDocs for use within document definitions.
  - Added support for locked down environments to ensure that documents are executed as constrained code when Device Guard is used.
    - Use the `Execution.LanguageMode = 'ConstrainedLanguage'` option to force constrained language mode.
  - **Important change**: Improved markdown formatting for tables. [#31](https://github.com/Microsoft/PSDocs/issues/31)
    - Table columns are now padded by default to match header width.
    Set `Markdown.ColumnPadding` option to `None` to match format style from PSDocs <= 0.5.0.
    - Pipe characters on the start and end of a table row are not added by default.
    Set `Markdown.UseEdgePipes` option to `Always` to match format style from PSDocs <= 0.5.0.
    - Property expressions now support Label, Expression, Alignment and Width keys.
  - **Experimental**: Publishing of keywords for syntax completion with editors.
- Deprecations and removals:
  - **Breaking change**: Removed `Import-PSDocumentTemplate` cmdlet. Use `Invoke-PSDocument` instead or dot source.
  - **Breaking change**: Removed support for `-Function` parameter of `Invoke-PSDocument`.
  External commands can be executed in document blocks. Re-evaluating if this is really needed.
  - **Important change**: Renamed `-When` parameter on Section block to `-If`.
    - This provides a shorter parameter and more clearly describes the intent of the parameter.
    - `-When` is still works but is deprecated.
  - **Important change**: Added support for `Note` and `Warning` keywords to accept text from the pipeline.
    - Using the pipeline is now the preferred way to use `Note` and `Warning` keywords.
    - `Note` and `Warning` script blocks are still work, but are deprecated.
- Bug fixes:
  - Fixed consistency of line break generation before and after document content.

## v0.5.0

What's changed since v0.4.0:

- Engine features:
  - Added support for property expressions with the `Table` keyword. [#27](https://github.com/Microsoft/PSDocs/issues/27)
  - Added support for building all document definitions from a path using the `-Path` parameter. [#25](https://github.com/Microsoft/PSDocs/issues/25)
    - Additionally document definitions can be filtered with the `-Name` and `-Tag` parameter.
    - This is the recommended way to build documents going forward.
  - Added support for providing options for `Invoke-PSDocument` using YAML.
    - See [about_PSDocs_Options] for more details.
- General improvements:
  - **Important change**: Deprecated support for using `Invoke-PSDocument` with inline document definitions.
    - Improved support for using named document definitions inline, use this to call inline document definitions.
  - **Breaking change**: Empty `Section` blocks are not rendered by default. [#32](https://github.com/Microsoft/PSDocs/issues/32)
    - Use `-Force` parameter on specific sections or `Markdown.SkipEmptySections = $False` option to force empty sections to be written.
- Bug fixes:
  - Fixed to prevent string builder properties being outputted each time `Invoke-PSDocument` is called.

## v0.4.0

What's changed since v0.3.0:

- Engine features:
  - Added `New-PSDocumentOption` cmdlet to configure document generation.
  - Added `-Option` parameter to `Invoke-PSDocument` cmdlet to accept configuration options.
- General improvements:
  - **Important change**: Renamed `Yaml` keyword to `Metadata`.
  `Yaml` keyword is still supported but deprecated, switch to using `Metadata` instead.
  - **Breaking change**: Added support for encoding markdown content output. [#16](https://github.com/Microsoft/PSDocs/issues/16)
    - To specify the encoding use the `-Encoding` parameter of `Invoke-PSDocument` and `Invoke-DscNodeDocument`.
    - Output now defaults to UTF-8 without byte order mark (BOM) instead of ASCII.
- Bug fixes:
  - Fixed handling of line break for multiline table columns using a wrap separator. [#11](https://github.com/Microsoft/PSDocs/issues/11)

## v0.3.0

What's changed since v0.2.0:

- Engine features:
  - Code blocks now generate fenced sections instead of indented sections.
- General improvements:
  - Improved `Yaml` block handling to allow YAML header to be defined throughout the document and merged when multiple blocks are defined.
  - Improved cmdlet help.
  - Output path is now automatically created by `Invoke-PSDocument` if it doesn't exist.
  - **Breaking change**: The body of Code blocks are now no longer evaluated as an expression.
    - This change improves editing of document templates, allowing editors to complete valid PowerShell syntax.
    - Define an expression and the pipe the results to the Code keyword to dynamically generate the contents of a code block.
- Engineering:
  - **Breaking change**: `-ConfigurationData` parameter of `Invoke-PSDocument` has been removed while purpose and future use of the parameter is reconsidered.
- Bug fixes:
  - Fixes to improve handling when `Title` block is used multiple times.
  - Fixes to prevent YAML header being created when `Yaml` block is not used.

## v0.2.0

What's changed since v0.1.0:

- Engine features:
  - Added Desired State Configuration (DSC) extension module `PSDocs.Dsc` to generate markdown from DSC `.mof` files.
  - Added support to include documentation from external script file.
- Engineering:
  - Moved markdown processor to a separate module.
- Bug fixes:
  - Fixed handling of multi-line notes and warnings.

## v0.1.0

- Initial release.

[about_PSDocs_Configuration]: docs/concepts/PSDocs/en-US/about_PSDocs_Configuration.md
[about_PSDocs_Conventions]: docs/concepts/PSDocs/en-US/about_PSDocs_Conventions.md
[about_PSDocs_Keywords]: docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md
[about_PSDocs_Options]: docs/concepts/PSDocs/en-US/about_PSDocs_Options.md
