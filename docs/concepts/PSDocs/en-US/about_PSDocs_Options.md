# PSDocs_Options

## about_PSDocs_Options

## SHORT DESCRIPTION

Describes additional options that can be used during markdown generation.

## LONG DESCRIPTION

PSDocs lets you define configuration parameters in a YAML file. This YAML file can be used with `Invoke-PSDocument` to quickly build documentation in a repeatable way.

## Markdown

This options control generation of markdown.

### Wrap Separator

Examples:

```powershell
$option = New-PSDocumentOption -Option @{ 'markdown.wrapseparator' = ' ' }
```

```yaml
- Markdown:
  - WrapSeparator: ' '
```

### Encoding

The encoding used for markdown output. One of the following values can be used:

- Default
- UTF8
- UTF7
- Unicode
- UTF32
- ASCII

Examples:

```powershell
$option = New-PSDocumentOption -Option @{ 'markdown.encoding' = 'UTF8' }
```

```yaml
- Markdown:
  - Encoding: UTF8
```

## EXAMPLES

```yaml
markdown:
  encoding: UTF8
  wrapSeparator: 'ZZZ'
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
