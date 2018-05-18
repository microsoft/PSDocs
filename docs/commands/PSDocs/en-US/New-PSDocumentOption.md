---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs/en-US/New-PSDocumentOption.md
schema: 2.0.0
---

# New-PSDocumentOption

## SYNOPSIS

Create options to configure document generation.

## SYNTAX

```text
New-PSDocumentOption [-Option <PSDocumentOption>] [-Path <String>] [<CommonParameters>]
```

## DESCRIPTION

The **New-PSDocumentOption** cmdlet creates an options object that can be passed to PSDocs cmdlets to configure document generation behaviour.

## EXAMPLES

### Example 1

```powershell
PS C:\> $option = New-PSDocumentOption -Option @{ 'Markdown.WrapSeparator' = '<br />' };
PS C:\> Invoke-PSDocument -Name 'Sample' -Option $option;
```

Create markdown using the Sample documentation definition using a wrap separator of `<br />`.

## PARAMETERS

### -Option

Additional options that configure document generation. Option also accepts a hashtable to configure options.

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

### -Path

The path to a YAML file containing options.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: .\.psdocs.yml
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable.
For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### PSDocs.Configuration.PSDocumentOption

## NOTES

## RELATED LINKS

[Invoke-PSDocument]

[Invoke-PSDocument]: Invoke-PSDocument.md
