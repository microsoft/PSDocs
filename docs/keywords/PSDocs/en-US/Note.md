---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/keywords/PSDocs/en-US/Node.md
schema: 2.0.0
---

# Note

## SYNOPSIS

Creates a formatted note block.

## SYNTAX

```text
Note [-Body] <ScriptBlock>
```

## EXAMPLES

### EXAMPLE 1

```powershell
Document 'Test' {

    Note {
        'This is a note.'
    }
}

Invoke-PSDocument -Name 'Test';
```

Generates a new Test.md document containing a block quote formatted as a DFM note.

## PARAMETERS

### -Body

A block of inline text to insert.

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
