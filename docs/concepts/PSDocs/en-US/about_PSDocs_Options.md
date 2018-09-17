# PSDocs_Options

## about_PSDocs_Options

## SHORT DESCRIPTION

Describes additional options that can be used during markdown generation.

## LONG DESCRIPTION

PSDocs lets you use options when calling `Invoke-PSDocument` to change how documents are generated. This topic describes what options are available, when to and how to use them.

Options can be used by:

- Using the `-Option` parameter of `Invoke-PSDocument` with an object created with `New-PSDocumentOption`
- Using the `-Option` parameter of `Invoke-PSDocument` with a hash table
- Using the `-Option` parameter of `Invoke-PSDocument` with a YAML file
- Configuring the default options file `.psdocs.yml`

As mentioned above, a options object can be created with `New-PSDocumentOption` see cmdlet help for syntax and examples.

When using a hash table, `@{}`, one or more options can be specified with the `-Option` parameter using a dotted notation.

For example:

```powershell
$option = @{ 'markdown.wrapseparator' = ' '; 'markdown.encoding' = 'UTF8' };
Invoke-PSDocument -Path . -Option $option;
```

`markdown.wrapseparator` is an example of an option that can be used. Please see the following sections for other options can be used.

Another option is to use an external file, formatted as YAML, instead of having to create an options object manually each time. This YAML file can be used with `Invoke-PSDocument` to quickly build documentation in a repeatable way.

YAML properties are specified using lower camel case, for example:

```yaml
markdown:
  wrapSeparator: '\'
```

By default PSDocs will automatically look for a file named `psdocs.yml` in the current working directory. Alternatively, you can specify a YAML file in the `-Option` parameter.

For example:

```powershell
Invoke-PSDocument -Path . -Option '.\myconfig.yml'.
```

### Wrap separator

This option specifies the character/string to use when wrapping lines in a table cell. When a table cell contains CR and LF characters, these characters must be substituted so that the table in rendered correctly because they also have special meaning in markdown.

By default a single space is used. However different markdown parsers may be able to natively render a line break using alternative combinations such as `\` or `<br />`.

This option can be specified using:

```powershell
# PowerShell: Using the Markdown.WrapSeparator hash table key
$option = New-PSDocumentOption -Option @{ 'Markdown.WrapSeparator' = '\' }
```

```yaml
# psdocs.yml: Using the markdown/wrapSeparator YAML property
markdown:
  wrapSeparator: '\'
```

### Encoding

Sets the text encoding used for markdown output files. One of the following values can be used:

- Default
- UTF8
- UTF7
- Unicode
- UTF32
- ASCII

By default `Default` is used which is UTF-8 without byte order mark (BOM) is used.

This option can be specified using:

```powershell
# PowerShell: Using the Markdown.Encoding hash table key
$option = New-PSDocumentOption -Option @{ 'Markdown.Encoding' = 'UTF8' }
```

```yaml
# psdocs.yml: Using the markdown/encoding YAML property
markdown:
  encoding: UTF8
```

Additionally `Invoke-PSDocument` has a `-Encoding` parameter. When the `-Encoding` parameter is used, it always takes precedence over an encoding set through `-Option` or `psdocs.yml`.

Prior to PSDocs v0.4.0 the only encoding supported was ASCII.

### Skip empty sections

From PSDocs v0.5.0 onward, `Section` blocks that are empty are omitted from markdown output by default. i.e. `Markdown.SkipEmptySections` is `$True`.

To include empty sections (PSDocs v0.4.0 or older) in markdown output either use the `-Force` parameter on a specific `Section` block or set `Markdown.SkipEmptySections = $False`.

This option can be specified using:

```powershell
# PowerShell: Using the Markdown.SkipEmptySections hash table key
$option = New-PSDocumentOption -Option @{ 'Markdown.SkipEmptySections' = $False }
```

```yaml
# psdocs.yml: Using the markdown/skipEmptySections YAML property
markdown:
  skipEmptySections: false
```

### Language mode

By default unless PowerShell has been constrained, full language features of PowerShell are available to use within document definitions. In locked down environments, a reduced set of language features may be desired.

When PSDocs is executed in an environment configured for Device Guard, only constrained language features are available.

The following language modes are available for use in PSDocs:

- FullLanguage
- ConstrainedLanguage

This option can be specified using:

```powershell
# PowerShell: Using the Execution.LanguageMode hash table key
$option = New-PSDocumentOption -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' }
```

```yaml
# psdocs.yml: Using the execution/languageMode YAML property
execution:
  languageMode: ConstrainedLanguage
```

## EXAMPLES

### Example PSDocs.yml

```yaml
# Set markdown options
markdown:
  # Use UTF-8 with BOM
  encoding: UTF8
  skipEmptySections: false
  wrapSeparator: '\'
execution:
  languageMode: ConstrainedLanguage
```

### Default PSDocs.yml

```yaml
# These are the default options.
# Only properties that differ from the default values need to be specified.
markdown:
  encoding: Default
  skipEmptySections: true
  wrapSeparator: ' '
execution:
  languageMode: FullLanguage
```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSDocs/blob/master/docs/concepts/PSDocs/en-US/about_PSDocs_Options.md.

## SEE ALSO

- [Invoke-PSDocument](https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs/en-US/Invoke-PSDocument.md)
- [New-PSDocumentOption](https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs/en-US/New-PSDocumentOption.md)

## KEYWORDS

- Options
- Markdown
- PSDocument
