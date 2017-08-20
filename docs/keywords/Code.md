
# Code

## SYNOPSIS
Creates a formatted code section.

## SYNTAX

```
Code [-Body] <ScriptBlock>
```

## EXAMPLES

### EXAMPLE 1

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
