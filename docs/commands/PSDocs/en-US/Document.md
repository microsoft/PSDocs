---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/main/docs/commands/PSDocs/en-US/Document.md
schema: 2.0.0
---

# Document

## SYNOPSIS

Defines a named block that can be called to output documentation.

## SYNTAX

```text
Document [-Name] <String> [-Body] <ScriptBlock>
```

## DESCRIPTION

Defines a named block that can be called to output documentation.

## EXAMPLES

### Example 1

```powershell
Document 'Test' {
    # Insert content here
}
```

Define an empty document template called Test.

## PARAMETERS

### -Body

A script block containing the document definition.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name

The name of the document.

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

### System.Object

## NOTES

## RELATED LINKS
