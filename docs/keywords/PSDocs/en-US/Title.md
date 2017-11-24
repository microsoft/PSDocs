---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/keywords/PSDocs/en-US/Title.md
schema: 2.0.0
---

# Title

## SYNOPSIS

Sets the title of the document.

## SYNTAX

```text
Title [-Content] <String>
```

## EXAMPLES

### EXAMPLE 1

```powershell
Document 'Test' {

    Title 'Test document'
}

Invoke-PSDocument -Name 'Test';
```

## PARAMETERS

### -Content

The title of the document.

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
