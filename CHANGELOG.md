
## Unreleased

- Add support for MacOS and Linux. [#59](https://github.com/BernieWhite/PSDocs/issues/59)
- **Breaking change**: Removed support for inline block
- **Breaking change**: Removed script block usage of `Note` and `Warning` previously deprecated.
  - Use pipeline instead.
- **Breaking change**: Removed support for `-When` section parameter previously deprecated.
  - Use `-If` parameter instead.

## v0.6.3

- Fix concatenation of multiple lines in Code section. [#69](https://github.com/BernieWhite/PSDocs/issues/69)

## v0.6.2

- Fix PositionMessage cannot be found on this object. [#63](https://github.com/BernieWhite/PSDocs/issues/63)
- Fix handling of null metadata hashtable. [#60](https://github.com/BernieWhite/PSDocs/issues/60)

## v0.6.1

- Fix null reference for table columns with undefined properties. [#53](https://github.com/BernieWhite/PSDocs/issues/53)

## v0.6.0

- Fix consistency of line break generation before and after document content.
- Added `BlockQuote` keyword to generate block quotes in addition to existing `Note` and `Warning` keywords which are specific to DocFX
  - See [about_PSDocs_Keywords](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#blockquote) help topic for details
- Added `-Culture` parameter to `Invoke-PSDocument`, which allows generation of multiple localized output files
- Added `Include` keyword to insert content from an external file
  - Use the `-UseCulture` switch of `Include` to insert content from a culture specific external file
  - See [about_PSDocs_Keywords](docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md#include) help topic for details
- Added support for locked down environments to ensure that documents are executed as constrained code when Device Guard is used
  - Use the `Execution.LanguageMode = 'ConstrainedLanguage'` option to force constrained language mode
- Added new contextual help topic to provide details on automatic variables exposed for PSDocs for use within document definitions
- **Breaking change**: Removed `Import-PSDocumentTemplate` cmdlet. Use `Invoke-PSDocument` instead or dot source
- **Breaking change**: Removed support for `-Function` parameter of `Invoke-PSDocument`. External commands can be executed in document blocks. Re-evaluating if this is really needed.
- **Important change**: Renamed `-When` parameter on Section block to `-If`. `-When` is still works but is deprecated.
- **Important change**: Improved markdown formatting for tables [#31](https://github.com/BernieWhite/PSDocs/issues/31)
  - Table columns are now padded by default to match header width. Set `Markdown.ColumnPadding` option to `None` to match format style from PSDocs <= 0.5.0
  - Pipe characters on the start and end of a table row are not added by default. Set `Markdown.UseEdgePipes` option to `Always` to match format style from PSDocs <= 0.5.0
  - Property expressions now support Label, Expression, Alignment and Width keys
- **Important change**: Added support for `Note` and `Warning` keywords to accept text from the pipeline
  - Using the pipeline is now the preferred way to use `Note` and `Warning` keywords
  - `Note` and `Warning` script blocks are still work, but are deprecated.
- **Experimental**: Publishing of keywords for syntax completion with editors

## v0.5.0

- Fix to prevent string builder properties being outputted each time `Invoke-PSDocument` is called
- Added support for property expressions with the `Table` keyword [#27](https://github.com/BernieWhite/PSDocs/issues/27)
- **Important change**: Deprecated support for using `Invoke-PSDocument` with inline document definitions
  - Improved support for using named document definitions inline, this is the recommended way to call inline document definitions
- Added support for building all document definitions from a path using the `-Path` parameter [#25](https://github.com/BernieWhite/PSDocs/issues/25)
  - Additionally document definitions can be filtered with the `-Name` and `-Tag` parameter
  - This is the recommended way to build documents going forward
- Added support for providing options for `Invoke-PSDocument` using YAML, see `about_PSDocs_Options`
- **Breaking change**: Empty `Section` blocks are not rendered by default [#32](https://github.com/BernieWhite/PSDocs/issues/32)
  - Use `-Force` parameter on specific sections or `Markdown.SkipEmptySections = $False` option to force empty sections to be written

## v0.4.0

- Fix handling of line break for multiline table columns using a wrap separator [#11](https://github.com/BernieWhite/PSDocs/issues/11)
- Added `New-PSDocumentOption` cmdlet to configure document generation
- Added `-Option` parameter to `Invoke-PSDocument` cmdlet to accept configuration options
- **Important change**: Renamed `Yaml` keyword to `Metadata`. `Yaml` keyword is still supported but deprecated, switch to using `Metadata` instead
- **Breaking change**: Added support for encoding markdown content output [#16](https://github.com/BernieWhite/PSDocs/issues/16)
  - To specify the encoding use the `-Encoding` parameter of `Invoke-PSDocument` and `Invoke-DscNodeDocument`
  - Output now defaults to UTF-8 without byte order mark (BOM) instead of ASCII

## v0.3.0

- Improved `Yaml` block handling to allow YAML header to be defined throughout the document and merged when multiple blocks are defined
- Improved cmdlet help
- Output path is automatically created by `Invoke-PSDocument` if it doesn't exist
- Fix to improve handling when `Title` block is used multiple times
- Fix to prevent YAML header being created when `Yaml` block is not used
- Code blocks now generate fenced sections instead of indented sections
- **Breaking change**: The body of Code blocks are now no longer evaluated as an expression
  - This change improves editing of document templates, allowing editors to complete valid PowerShell syntax
  - Define an expression and the pipe the results to the Code keyword to dynamically generate the contents of a code block
- **Breaking change**: `-ConfigurationData` parameter of `Invoke-PSDocument` has been removed while purpose and future use of the parameter is reconsidered

## v0.2.0

- Added Desired State Configuration (DSC) extension module `PSDocs.Dsc` to generate markdown from DSC `.mof` files
- Moved markdown processor to a separate module
- Fix handling of multi-line notes and warnings
- Added support to include documentation from external script file

## v0.1.0

- Initial release
