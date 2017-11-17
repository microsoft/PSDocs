---
external help file: PSDocs.Dsc-help.xml
Module Name: PSDocs.Dsc
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs.Dsc/en-US/Get-DscMofDocument.md
schema: 2.0.0
---

# Get-DscMofDocument

## SYNOPSIS

Get document graph from .mof file.

## SYNTAX

```text
Get-DscMofDocument [-Path] <String>
```

## DESCRIPTION

Get a Managed Object Format (MOF) graph from a .mof file.

This cmdlet will return a DscMofDocument object, containing ResourceId and ResourceType properties that enumerate Desired State Configuration (DSC) resources defined in the .mof file.

## EXAMPLES

### Example 1

```powershell
PS C:\> Get-DscMofDocument -Path '.\node1.mof';
```

Get resources from a .mof file.

## PARAMETERS

### -Path

The file path to a .mof file.

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

## INPUTS

### None

## OUTPUTS

### DscMofDocument

## NOTES

## RELATED LINKS
