---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs/en-US/Get-PSDocumentHeader.md
schema: 2.0.0
---

# Get-PSDocumentHeader

## SYNOPSIS

Get the Yaml header from a PSDocs generated markdown file.

## SYNTAX

```text
Get-PSDocumentHeader [[-Path] <String>]
```

## DESCRIPTION

Get the Yaml header from a PSDocs generated markdown file.

## EXAMPLES

### Example 1

```powershell
PS C:\> Get-PSDocumentHeader -Path '.\build\Default';
```

Get the Yaml header for all markdown files in the Default directory.

### Example 2

```powershell
PS C:\> Get-PSDocumentHeader -Path '.\build\Default\Server1.md';
```

Get the Yaml header for a specific file Server1.md.

### Example 3

```powershell
PS C:\> Get-PSDocumentHeader;
```

Get the Yaml header for all markdown files in the current working directory.

## PARAMETERS

### -Path

The path to a specific markdown file or a parent directory containing one or more markdown files. A trailing slash is not required.

If a path is not specified the current working path will be used.

```yaml
Type: String
Parameter Sets: (All)
Aliases: FullName

Required: False
Position: 0
Default value: $PWD
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

## INPUTS

### System.String

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
