---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/keywords/PSDocs/en-US/Code.md
schema: 2.0.0
---

# Code

## SYNOPSIS

Creates a formatted code section.

## SYNTAX

```text
Code [-Info <String>] [-Body] <ScriptBlock>
```

## DESCRIPTION

Creates a formatted code section.

## EXAMPLES

### Example 1

```powershell
Document 'Test' {

    Code {
        powershell.exe -Help
    }
}

Invoke-PSDocument -Name 'Test' -InputObject $Null;
```

Generates a new Test.md document containing the powershell.exe -Help command line.

### Example 2

```powershell
Document 'Test' {

    Code powershell {
        Get-Item -Path .\;
    }
}

Invoke-PSDocument -Name 'Test' -InputObject $Null;
```

Generates a new Test.md document containing script code formatted with the powershell info string.

## PARAMETERS

### -Info

A markdown info string used to identify the script language.

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

### -Body

A block of inline code to insert.

```yaml
Type: ScriptBlock
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
