---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs/en-US/Import-PSDocumentTemplate.md
schema: 2.0.0
---

# Import-PSDocumentTemplate

## SYNOPSIS

Import a document template script file into the current environment.

## SYNTAX

```text
Import-PSDocumentTemplate [-Path] <String> [<CommonParameters>]
```

## DESCRIPTION

Import a document template script file into the current environment.

## EXAMPLES

### Example 1

```powershell
PS C:\> Import-PSDocumentTemplate -Path '.\template.ps1';
```

Imports a document template into the current environment.

## PARAMETERS

### -Path

The file path to a script file containing a documentation template.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
