# PSDocs_Options

## about_PSDocs_Options

## SHORT DESCRIPTION

Describes additional options that can be used during markdown generation.

## LONG DESCRIPTION

PSDocs lets you use options when calling `Invoke-PSDocument` to change how documents are generated.
This topic describes what options are available, when to and how to use them.

The following workspace options are available for use:

- [Configuration](#configuration)
- [Execution.LanguageMode](#executionlanguagemode)
- [Markdown.ColumnPadding](#markdowncolumnpadding)
- [Markdown.Encoding](#markdownencoding)
- [Markdown.SkipEmptySections](#markdownskipemptysections)
- [Markdown.UseEdgePipes](#markdownuseedgepipes)
- [Markdown.WrapSeparator](#markdownwrapseparator)
- [Output.Culture](#outputculture)
- [Output.Path](#outputpath)

Options can be used by:

- Using the `-Option` parameter of `Invoke-PSDocument` with an object created with `New-PSDocumentOption`.
- Using the `-Option` parameter of `Invoke-PSDocument` with a hashtable.
- Using the `-Option` parameter of `Invoke-PSDocument` with a YAML file.
- Configuring the default options file `ps-docs.yaml`.

As mentioned above, a options object can be created with `New-PSDocumentOption` see cmdlet help for syntax and examples.
When using a hashtable, `@{}`, one or more options can be specified with the `-Option` parameter using a dotted notation.

For example:

```powershell
$option = @{ 'markdown.wrapseparator' = ' '; 'markdown.encoding' = 'UTF8' };
Invoke-PSDocument -Path . -Option $option;
```

`markdown.wrapseparator` is an example of an option that can be used.
Please see the following sections for other options can be used.

Another option is to use an external file, formatted as YAML, instead of having to create an options object manually each time.
This YAML file can be used with `Invoke-PSDocument` to quickly build documentation in a repeatable way.

YAML properties are specified using lower camel case, for example:

```yaml
markdown:
  wrapSeparator: '\'
```

By default PSDocs will automatically look for a file named `ps-docs.yaml` in the current working directory.
Alternatively, you can specify a YAML file in the `-Option` parameter.

For example:

```powershell
Invoke-PSDocument -Path . -Option '.\myconfig.yml'.
```

### Configuration

Sets custom configuration for document generation.
Document definitions may allow custom configuration to be specified.
To specify custom configuration, set a property of the configuration object.

To ensure each custom key is unique use a prefix followed by an underscore that represent your module.
Key names are not case sensitive, however we recommend you use uppercase for consistency.

This option can be specified using:

```powershell
# PowerShell: Using the Configuration hashtable key to set custom configuration 'MODULE1_KEY1'
$option = New-PSDocumentOption -Option @{ 'Configuration.MODULE1_KEY1' = 'Value1' }
```

```yaml
# YAML: Using the configuration YAML property to set custom configuration 'MODULE2_KEY1'
configuration:
  MODULE2_KEY1: Value1
```

### Execution.LanguageMode

Unless PowerShell has been constrained, full language features of PowerShell are available to use within document definitions.
In locked down environments, a reduced set of language features may be desired.

When PSDocs is executed in an environment configured for Device Guard, only constrained language features are available.

The following language modes are available for use in PSDocs:

- FullLanguage
- ConstrainedLanguage

This option can be specified using:

```powershell
# PowerShell: Using the Execution.LanguageMode hashtable key
$option = New-PSDocumentOption -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' }
```

```yaml
# YAML: Using the execution/languageMode YAML property
execution:
  languageMode: ConstrainedLanguage
```

### Input.Format

Configures the input format for when a string is passed in as a target object.
This option determines if the target object is deserialized into an alternative form.

Set this option to either `Yaml`, `Json`, `PowerShellData` to deserialize as a specific format.
The `-Format` parameter will override any value set in configuration.

When the `-InputObject` parameter or pipeline input is used, strings are treated as plain text by default.
`FileInfo` objects for supported file formats will be deserialized based on file extension.

When the `-InputPath` parameter is used, supported file formats will be deserialized based on file extension.
The `-InputPath` parameter can be used with a file path or URL.

The following formats are available:

- None - Treat strings as plain text and do not deserialize files.
- Yaml - Deserialize as one or more YAML objects.
- Json - Deserialize as one or more JSON objects.
- PowerShellData - Deserialize as a PowerShell data object.
- Detect - Detect format based on file extension. This is the default.

If the `Detect` format is used, the file extension will be used to automatically detect the format.
When the file extension can not be determined `Detect` is the same as `None`.

Detect uses the following file extensions:

- Yaml - `.yaml` or `.yml`
- Json - `.json` or `.jsonc`
- PowerShellData - `.psd1`

This option can be specified using:

```powershell
# PowerShell: Using the Format parameter
$option = New-PSDocumentOption -Format Yaml;
```

```powershell
# PowerShell: Using the Input.Format hashtable key
$option = New-PSDocumentOption -Option @{ 'Input.Format' = 'Yaml' };
```

```yaml
# YAML: Using the input/format property
input:
  format: Yaml
```

```bash
# Bash: Using environment variable
export PSDOCS_INPUT_FORMAT=Yaml
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSDOCS_INPUT_FORMAT: Yaml
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSDOCS_INPUT_FORMAT
  value: Yaml
```

### Input.ObjectPath

The object path to a property to use instead of the pipeline object.

By default, PSDocs processes objects passed from the pipeline against selected rules.
When this option is set, instead of evaluating the pipeline object, PSDocs looks for a property of the pipeline object specified by `ObjectPath` and uses that instead.
If the property specified by `ObjectPath` is a collection/ array, then each item is evaluated separately.

If the property specified by `ObjectPath` does not exist, PSDocs skips the object.

This option can be specified using:

```powershell
# PowerShell: Using the InputObjectPath parameter
$option = New-PSDocumentOption -InputObjectPath 'items';
```

```powershell
# PowerShell: Using the Input.ObjectPath hashtable key
$option = New-PSDocumentOption -Option @{ 'Input.ObjectPath' = 'items' };
```

```yaml
# YAML: Using the input/objectPath property
input:
  objectPath: items
```

```bash
# Bash: Using environment variable
export PSDOCS_INPUT_OBJECTPATH=items
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSDOCS_INPUT_OBJECTPATH: items
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSDOCS_INPUT_OBJECTPATH
  value: items
```

### Input.PathIgnore

Ignores input files that match the path spec when using `-InputPath`.
If specified, files that match the path spec will not be processed.
By default, all files are processed.

This option can be specified using:

```powershell
# PowerShell: Using the InputPathIgnore parameter
$option = New-PSDocumentOption -InputPathIgnore '*.Designer.cs';
```

```powershell
# PowerShell: Using the Input.PathIgnore hashtable key
$option = New-PSDocumentOption -Option @{ 'Input.PathIgnore' = '*.Designer.cs' };
```

```yaml
# YAML: Using the input/pathIgnore property
input:
  pathIgnore:
  - '*.Designer.cs'
```

```bash
# Bash: Using environment variable
export PSDOCS_INPUT_PATHIGNORE=*.Designer.cs
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSDOCS_INPUT_PATHIGNORE: '*.Designer.cs'
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSDOCS_INPUT_PATHIGNORE
  value: '*.Designer.cs'
```

### Markdown.ColumnPadding

Sets how table column padding should be handled in markdown.
This doesn't affect how tables are rendered but can greatly assist readability of markdown source files.

The following padding options are available:

- None - No padding will be used and column values will directly follow table pipe (`|`) column separators.
- Single - A single space will be used to pad the column value.
- MatchHeader - Will pad the header with a single space, then pad the column value, to the same width as the header (default).

When a column is set to a specific width with a property expression, `MatchHeader` will be ignored.
Columns without a width set will apply `MatchHeader` as normal.

Example markdown using `None`:

```markdown
|Name|Value|
|----|-----|
|Mon|Key|
|Really long name|Really long value|
```

Example markdown using `Single`:

```markdown
| Name | Value |
| ---- | ----- |
| Mon | Key |
| Really long name | Really long value |
```

Example markdown using `MatchHeader`:

```markdown
| Name | Value |
| ---- | ----- |
| Mon  | Key   |
| Really long name | Really long value |
```

This option can be specified using:

```powershell
# PowerShell: Using the Markdown.ColumnPadding hashtable key
$option = New-PSDocumentOption -Option @{ 'Markdown.ColumnPadding' = 'MatchHeader' }
```

```yaml
# YAML: Using the markdown/columnPadding YAML property
markdown:
  columnPadding: MatchHeader
```

### Markdown.Encoding

Sets the text encoding used for markdown output files.
One of the following values can be used:

- Default
- UTF8
- UTF7
- Unicode
- UTF32
- ASCII

By default `Default` is used which is UTF-8 without byte order mark (BOM) is used.

Use this option with `Invoke-PSDocument`.
When the `-Encoding` parameter is used, it will override any value set in configuration.

This option can be specified using:

```powershell
# PowerShell: Using the Encoding parameter
$option = New-PSDocumentOption -Encoding 'UTF8';
```

```powershell
# PowerShell: Using the Markdown.Encoding hashtable key
$option = New-PSDocumentOption -Option @{ 'Markdown.Encoding' = 'UTF8' }
```

```yaml
# YAML: Using the markdown/encoding YAML property
markdown:
  encoding: UTF8
```

Prior to PSDocs v0.4.0 the only encoding supported was ASCII.

### Markdown.SkipEmptySections

From PSDocs v0.5.0 onward, `Section` blocks that are empty are omitted from markdown output by default.
i.e. `Markdown.SkipEmptySections` is `$True`.

To include empty sections (the same as PSDocs v0.4.0 or older) in markdown output either use the `-Force` parameter on a specific `Section` block or set the option `Markdown.SkipEmptySections` to `$False`.

This option can be specified using:

```powershell
# PowerShell: Using the Markdown.SkipEmptySections hashtable key
$option = New-PSDocumentOption -Option @{ 'Markdown.SkipEmptySections' = $False }
```

```yaml
# YAML: Using the markdown/skipEmptySections YAML property
markdown:
  skipEmptySections: false
```

### Markdown.UseEdgePipes

This option determines when pipes on the edge of a table should be used.
This option can improve readability of markdown source files, but may not be supported by all markdown renderers.

Edge pipes are always required if the table has a single column, so this option only applies for tables with more then one column.

The following options for edge pipes are:

- WhenRequired - Will not use edge pipes for tables with more when one column (default).
- Always - Will always use edge pipes.

Example markdown using `WhenRequired`:

```markdown
Name|Value
----|-----
Mon|Key
```

Example markdown using `Always`:

```markdown
|Name|Value|
|----|-----|
|Mon|Key|
```

Example markdown using `WhenRequired` and column padding of `MatchHeader`:

```markdown
Name | Value
---- | -----
Mon  | Key
```

This option can be specified using:

```powershell
# PowerShell: Using the Markdown.UseEdgePipes hashtable key
$option = New-PSDocumentOption -Option @{ 'Markdown.UseEdgePipes' = 'WhenRequired' }
```

```yaml
# YAML: Using the markdown/useEdgePipes YAML property
markdown:
  useEdgePipes: WhenRequired
```

### Markdown.WrapSeparator

This option specifies the character/string to use when wrapping lines in a table cell.
When a table cell contains CR and LF characters, these characters must be substituted so that the table in rendered correctly because they also have special meaning in markdown.

By default a single space is used.
However different markdown parsers may be able to natively render a line break using alternative combinations such as `\` or `<br />`.

This option can be specified using:

```powershell
# PowerShell: Using the Markdown.WrapSeparator hashtable key
$option = New-PSDocumentOption -Option @{ 'Markdown.WrapSeparator' = '\' }
```

```yaml
# YAML: Using the markdown/wrapSeparator YAML property
markdown:
  wrapSeparator: '\'
```

### Output.Culture

Specifies a list of cultures for building documents such as _en-AU_, and _en-US_.
Documents are written to culture specific subdirectories when multiple cultures are generated.

Use this option with `Invoke-PSDocument`.
When the `-Culture` parameter is used, it will override any value set in configuration.

This option can be specified using:

```powershell
# PowerShell: Using the Output.Culture hashtable key
$option = New-PSDocumentOption -Option @{ 'Output.Culture' = 'en-US', 'en-AU' }
```

```yaml
# YAML: Using the output/culture YAML property
output:
  culture:
  - 'en-US'
```

### Output.Path

Configures the directory path to store markdown files created based on the specified document template.
This path will be automatically created if it doesn't exist.

Use this option with `Invoke-PSDocument`.
When the `-OutputPath` parameter is used, it will override any value set in configuration.

This option can be specified using:

```powershell
# PowerShell: Using the Output.Path hashtable key
$option = New-PSDocumentOption -Option @{ 'Output.Path' = 'out/' }
```

```yaml
# YAML: Using the output/path YAML property
output:
  path: 'out/'
```

## EXAMPLES

### Example ps-docs.yaml

```yaml
configuration:
  SAMPLE_USE_PARAMETERS_SNIPPET: true

execution:
  languageMode: ConstrainedLanguage

# Set markdown options
markdown:
  # Use UTF-8 with BOM
  encoding: UTF8
  skipEmptySections: false
  wrapSeparator: '\'

output:
  culture:
  - 'en-US'
  path: 'out/'
```

### Default ps-docs.yaml

```yaml
# These are the default options.
# Only properties that differ from the default values need to be specified.
configuration: { }

execution:
  languageMode: FullLanguage

markdown:
  encoding: Default
  skipEmptySections: true
  wrapSeparator: ' '
  columnPadding: MatchHeader
  useEdgePipes: WhenRequired

output:
  culture: [ ]
  path: null
```

## NOTE

An online version of this document is available at https://github.com/Microsoft/PSDocs/blob/main/docs/concepts/PSDocs/en-US/about_PSDocs_Options.md.

## SEE ALSO

- [Invoke-PSDocument](https://github.com/Microsoft/PSDocs/blob/main/docs/commands/PSDocs/en-US/Invoke-PSDocument.md)
- [New-PSDocumentOption](https://github.com/Microsoft/PSDocs/blob/main/docs/commands/PSDocs/en-US/New-PSDocumentOption.md)

## KEYWORDS

- Configuration
- Options
- Markdown
- PSDocument
- Output
- Execution
