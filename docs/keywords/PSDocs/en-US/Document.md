---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/keywords/PSDocs/en-US/Document.md
schema: 2.0.0
---

# Document

## SYNOPSIS

Defines a named block that can be called to output documentation.

## SYNTAX

```text
Document [-Name] <String>
```

## EXAMPLES

### EXAMPLE 1

```powershell
Document 'Test' {
    # Insert content here
}
```

## PARAMETERS

### -Name

The name of the document.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
