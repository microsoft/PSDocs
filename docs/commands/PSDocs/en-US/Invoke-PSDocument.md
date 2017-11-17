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
Invoke-PSDocument [-Name] <String> [-InstanceName <String[]>] [-InputObject <PSObject>]
 [-ConfigurationData <Object>] [-Path <String>] [-OutputPath <String>]
 [-Function <System.Collections.Generic.Dictionary`2[System.String,System.Management.Automation.ScriptBlock]>]
 [-PassThru]
```

## DESCRIPTION

Create markdown from an input object.

## EXAMPLES

### Example 1

```powershell
PS C:\> Invoke-PSDocument -Name 'Sample' -InputObject 'C:\';
```

Create markdown using the Sample documentation template for 'C:\'.

## PARAMETERS

### -ConfigurationData

{{Fill ConfigurationData Description}}

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

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

{{Fill InstanceName Description}}

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

The directory path to store markdown files created based on the specified document template.

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

{{Fill PassThru Description}}

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

### -Path

{{Fill Path Description}}

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

## INPUTS

### System.Management.Automation.PSObject

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
