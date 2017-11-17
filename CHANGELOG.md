
## Unreleased

- Improved `Yaml` block handling to allow yaml header to be defined throughout the document and merged when multiple blocks are defined
- Improved cmdlet help
- Fix to improve handling when `Title` block is used multiple times
- Fix to prevent yaml header being created when `Yaml` block is not used

## v0.2.0

- Added Desired State Configuration (DSC) extension module PSDocs.Dsc to generate markdown from DSC .mof files
- Moved markdown processor to a seperate module
- Fix handling of multi-line notes and warnings
- Added support to include documentation from external script file

## v0.1.0

- Initial release