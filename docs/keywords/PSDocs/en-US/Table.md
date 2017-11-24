---
external help file: PSDocs-help.xml
Module Name: PSDocs
online version: https://github.com/BernieWhite/PSDocs/blob/master/docs/keywords/PSDocs/en-US/Table.md
schema: 2.0.0
---

# Table

## SYNOPSIS

Creates a formatted table.

## SYNTAX

```text
Table [-Property <String[]>]
```

## EXAMPLES

### EXAMPLE 1

```powershell
Document 'Test' {

    Section 'Directory list' {
        Get-ChildItem -Path 'C:\' | Table -Property Name,PSIsContainer;
    }
}

Invoke-PSDocument -Name 'Test';
```

Generates a new Test.md document containing a table populated with a row for each item. Only the properties Name and PSIsContainer are added as columns.