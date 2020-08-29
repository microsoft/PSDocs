---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/main/docs/commands/PSDocs/en-US/Get-PSDocument.md
schema: 2.0.0
---

# Get-PSDocument

## SYNOPSIS

Get document definitions.

## SYNTAX

```text
Get-PSDocument [-Module <String[]>] [-ListAvailable] [-Name <String[]>] [[-Path] <String[]>]
 [-Option <PSDocumentOption>] [<CommonParameters>]
```

## DESCRIPTION

Gets a list of document definitions from paths and modules.
Document definitions are discovered within files ending in `.Doc.ps1`.
By default, definitions will be be discovered from the current working path.
Use `-Module` to discover definitions from modules.

A document is defined using the `Document` keyword.

## EXAMPLES

### Example 1

```powershell
Get-PSDocument;
```

Get a list of document definitions from the current working path.

## PARAMETERS

### -Module

List document definitions in the specified modules.
When specified, only document definitions from modules will be listed.
To additionally list document definitions in paths use `-Path` together with `-Module`.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: m

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ListAvailable

Get document definitions from all modules even ones that are not imported.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

A list of paths to check for document definitions.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: p

Required: False
Position: 1
Default value: $PWD
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name

The name of specific document definitions to return.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: n

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Option

Additional options that configure PSDocs.
A `PSDocumentOption` can be created by using the `New-PSDocumentOption` cmdlet.
Alternatively a hashtable or path to YAML file can be specified with options.

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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### PSDocs.Definitions.IDocumentDefinition

An instance of a document definition.

## NOTES

## RELATED LINKS

[Invoke-PSDocument](Invoke-PSDocument.md)
