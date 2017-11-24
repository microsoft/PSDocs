---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/keywords/PSDocs/en-US/Yaml.md
schema: 2.0.0
---

# Yaml

## SYNOPSIS

Creates a yaml header.

## SYNTAX

```text
Yaml [-Body] <Hashtable>
```

## EXAMPLES

### EXAMPLE 1

```powershell
Document 'Test' {

    Yaml @{
        title = 'An example title'
    }

    'Yaml header may not be rendered by some markdown viewers. See source to view yaml.'
}

Invoke-PSDocument -Name 'Test';
```

Generates a new Test.md document containing a yaml header. An example of the output generated is available [here](/docs/examples/Yaml-header-output.md).

## PARAMETERS

### -Body

A hashtable containing header key/values.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```
