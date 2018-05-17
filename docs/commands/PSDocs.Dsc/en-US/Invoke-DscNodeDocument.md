---
external help file: PSDocs.Dsc-help.xml
Module Name: PSDocs.Dsc
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs.Dsc/en-US/Invoke-DscNodeDocument.md
schema: 2.0.0
---

# Invoke-DscNodeDocument

## SYNOPSIS

Create markdown from Desired State Configuration (DSC) .mof file.

## SYNTAX

```text
Invoke-DscNodeDocument [[-DocumentName] <String>] [[-Script] <String>] [[-InstanceName] <String[]>]
 [[-Path] <String>] [[-OutputPath] <String>] [<CommonParameters>]
```

## DESCRIPTION

Create markdown from Desired State Configuration (DSC) .mof file.

## EXAMPLES

### Example 1

```powershell
# Define a document called Sample
Document 'Sample' {

    # Add an 'Installed features' section in the document
    Section 'Installed features' {
        # Add a comment
        'The following Windows features have been installed.'

        # Generate a table of Windows Features
        $InputObject.ResourceType.WindowsFeature | Table -Property Name,Ensure
    }
}

Invoke-DscNodeDocument -DocumentName 'Sample' -Path '.\nodes' -OutputPath '.\docs';
```

Generates a new markdown document for each node .mof in the path '.\nodes'.

## PARAMETERS

### -DocumentName

The name of the document template.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InstanceName

The name(s) of the .mof instance to create markdown for. If this option is not specified, markdown will be created for all instances.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputPath

The directory path to store generated markdown files.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 4
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

The path to search for .mof files in.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Script

A file path to a script file containing the documentation template to use.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
