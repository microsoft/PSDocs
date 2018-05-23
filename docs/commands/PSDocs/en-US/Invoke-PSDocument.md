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

```text
Invoke-PSDocument [-Name] <String> [-InstanceName <String[]>] [-InputObject <PSObject>] [-OutputPath <String>]
 [-Function <System.Collections.Generic.Dictionary`2[System.String,System.Management.Automation.ScriptBlock]>]
 [-PassThru] [-Option <PSDocumentOption>] [-Encoding <MarkdownEncoding>] [<CommonParameters>]
```

## DESCRIPTION

Create markdown from an input object using a document definition. A document is defined using the Document keyword.

## EXAMPLES

### Example 1

```powershell
# Define a document called Sample
Document Sample {

    # Add an introduction section
    Section Introduction {

        # Add a comment
        "This is a sample file list from $InputObject"

        # Generate a table
        Get-ChildItem -Path $InputObject | Table -Property Name,PSIsContainer
    }
}

Invoke-PSDocument -Name 'Sample' -InputObject 'C:\';
```

Create markdown using the Sample documentation definition for 'C:\'.

## PARAMETERS

### -Function

This option can be used to define documentation specific functions that can be used using markdown creation.

```yaml
Type: System.Collections.Generic.Dictionary`2[System.String,System.Management.Automation.ScriptBlock]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject

An input object that will be used to build markdown.

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

The name of the resulting markdown file. During execution of this command, the variable $InstanceName will be available within the document definition for use by expressions.

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

The name of a specific document template to use to generate markdown.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
