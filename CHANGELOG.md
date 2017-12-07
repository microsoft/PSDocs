
## Unreleased

## v0.3.0

- Improved `Yaml` block handling to allow yaml header to be defined throughout the document and merged when multiple blocks are defined
- Improved cmdlet help
- Output path is automatically created by `Invoke-PSDocument` if it doesn't exist
- Fix to improve handling when `Title` block is used multiple times
- Fix to prevent yaml header being created when `Yaml` block is not used
- Code blocks now generate fenced sections instead of indented sections
- [Breaking change] The body of Code blocks are now no longer evaluated as an expression
  - This change improves editing of document templates, allowing editors to complete valid PowerShell syntax
  - Define an expression and the pipe the results to the Code keyword to dynamically generate the contents of a code block
- [Breaking change] `-ConfigurationData` parameter of `Invoke-PSDocument` has been removed while purpose and future use of the parameter is reconsidered

## v0.2.0

- Added Desired State Configuration (DSC) extension module PSDocs.Dsc to generate markdown from DSC .mof files
- Moved markdown processor to a seperate module
- Fix handling of multi-line notes and warnings
- Added support to include documentation from external script file

## v0.1.0

- Initial release