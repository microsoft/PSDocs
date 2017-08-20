
# Section

## SYNOPSIS
Creates a new document section that contains content.

## SYNTAX

```
Section [-Name] <String> [-When <ScriptBlock>]
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

Generates a new Test.md document containing a table listing all items directly within C:\.

### EXAMPLE 2

```powershell
Document 'Test' {

    Section 'Directory list' -When { Test-Path -Path 'C:\' } {
        Get-ChildItem -Path 'C:\' | Table -Property Name,PSIsContainer;
    }
}

Invoke-PSDocument -Name 'Test';
```

Generates a new Test.md document containing a table listing all items directly within C:\, but only if C:\ exists.

## PARAMETERS

### -Name
The name of the section.

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

### -When
An optional condition that must be met before the section is included.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases: 

Required: False
Position: None
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

