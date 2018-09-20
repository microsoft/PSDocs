---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs/en-US/Invoke-PSDocument.md
schema: 2.0.0
---

# Invoke-PSDocument

## SYNOPSIS

Create markdown from an input object.

## SYNTAX

### Inline (Default)

```text
Invoke-PSDocument -Name <String[]> [-InstanceName <String[]>] [-InputObject <PSObject>] [-OutputPath <String>]
 [-PassThru] [-Option <PSDocumentOption>] [-Encoding <MarkdownEncoding>] [<CommonParameters>]
```

### Path

```text
Invoke-PSDocument [-Name <String[]>] [-Tag <String[]>] [-InstanceName <String[]>] [-InputObject <PSObject>]
 [-Path] <String> [-OutputPath <String>]
 [-PassThru] [-Option <PSDocumentOption>] [-Encoding <MarkdownEncoding>] [<CommonParameters>]
```

## DESCRIPTION

Create markdown from an input object using a document definition. A document is defined using the `document` keyword.

## EXAMPLES

### Example 1

```powershell
# Create a new document definition called Sample in sample.doc.ps1
Set-Content -Path .\sample.doc.ps1 -Value @'
document Sample {

    # Add an introduction section
    Section Introduction {

        # Add a comment
        "This is a sample file list from $InputObject"

        # Generate a table
        Get-ChildItem -Path $InputObject | Table -Property Name,PSIsContainer
    }
}
'@

# Discover document definitions in the current working path (and subdirectories) within .doc.ps1 files
Invoke-PSDocument -Path .;
```

Create markdown using *.doc.ps1 files loaded from the current working directory.

### Example 2

```powershell
# Define an inline document called Sample
document Sample {

    # Add an introduction section
    Section Introduction {

        # Add a comment
        "This is a sample file list from $InputObject"

        # Generate a table
        Get-ChildItem -Path $InputObject | Table -Property Name,PSIsContainer
    }
}

# Calling an inline document definition by name using Invoke-PSDocument is depricated
Invoke-PSDocument -Name 'Sample' -InputObject 'C:\';

# This is recommended way to call Sample
Sample -InputObject 'C:\';
```

Create markdown using the inline documentation definition called Sample using as input 'C:\'.

## PARAMETERS

### -InputObject

An input object that will be passed to each document and can be referenced within document blocks as `$InputObject`.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -InstanceName

The name of the resulting markdown file. During execution of this command, the variable `$InstanceName` will be available within the document definition for use by expressions.

If InstanceName is not specified the name of the document definition will be used instead.

If more then one InstanceName is specified, multiple markdown files will be generated in the order they were specified.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name

The name of a specific document definition to use to generate markdown.

When this option is used with `-Path`, script files will be executed in order, and document blocks that do not match name are skipped.

```yaml
Type: String[]
Parameter Sets: Inline
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String[]
Parameter Sets: Path
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputPath

The directory path to store markdown files created based on the specified document template. This path will be automatically created if it doesn't exist.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru

When specified generated markdown will be returned to the pipeline instead of being written to file.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Option

Additional options that configure document generation. A `PSDocumentOption` can be created by using the `New-PSDocumentOption` cmdlet. Alternatively a hashtable or path to YAML file can be specified with options.

```yaml
Type: PSDocumentOption
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Encoding

Specifies the file encoding for generated markdown files. By default, UTF-8 without byte order mark (BOM) will be used. To use UTF-8 with BOM specify `UTF8`.

```yaml
Type: MarkdownEncoding
Parameter Sets: (All)
Aliases:
Accepted values: Default, UTF8, UTF7, Unicode, UTF32, ASCII

Required: False
Position: Named
Default value: Default
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

A directory path to read document definitions recursively from. Document definitions are discovered within files ending in `.doc.ps1`.

```yaml
Type: String
Parameter Sets: Path
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Tag

One or more tags that the document definition must contain. If more then one tag is specified, all tags be present on the document definition to be evaluated.

```yaml
Type: String[]
Parameter Sets: Path
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
