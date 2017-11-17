---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs/en-US/Document.md
schema: 2.0.0
---

# Code

## SYNOPSIS

Creates a formatted code section.

## SYNTAX

```text
Code [-Body] <ScriptBlock>
```

## DESCRIPTION

Creates a formatted code section.

## EXAMPLES

### Example 1

```powershell
Document 'Test' {

    Code {
        'Get-Item -Path .\;'
    }
}

Invoke-PSDocument -Name 'Test' -InputObject $Null;
```

Generates a new Test.md document containing code.

## PARAMETERS

### -Body

A block of inline code to insert.

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

## INPUTS

### None

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
