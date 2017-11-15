---
external help file: PSDocs.Dsc-help.xml
Module Name: PSDocs.Dsc
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs.Dsc/en-US/Invoke-DscNodeDocument.md
schema: 2.0.0
---

# Invoke-DscNodeDocument

## SYNOPSIS

Calls a document definition.

## SYNTAX

```text
Invoke-DscNodeDocument [[-DocumentName] <String>] [[-Script] <String>] [[-InstanceName] <String[]>]
 [[-Path] <String>] [[-OutputPath] <String>]
```

## DESCRIPTION

{{Fill in the Description}}

## EXAMPLES

### Example 1

```powershell
PS C:\> {{ Add example code here }}
```

```powershell
Document 'Test' {

    Section 'Installed features' {
        'The following Windows features have been installed.'

        $InputObject.ResourceType.WindowsFeature | Table -Property Name,Ensure;
    }
}

Invoke-DscNodeDocument -DocumentName 'Test' -Path '.\nodes' -OutputPath '.\docs';
```

Generates a new markdown document for each node .mof in the path `.\nodes`.

## PARAMETERS

### -DocumentName

{{Fill DocumentName Description}}

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

{{Fill InstanceName Description}}

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

{{Fill OutputPath Description}}

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

{{Fill Path Description}}

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

{{Fill Script Description}}

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

## INPUTS

### None

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
