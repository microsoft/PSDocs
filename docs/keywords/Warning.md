
# Warning

## SYNOPSIS
Creates a formatted warning block.

## SYNTAX

```
Warning [-Body] <ScriptBlock>
```

## EXAMPLES

### EXAMPLE 1

```powershell
Document 'Test' {

    Warning {
        'This is a warning.'
    }
}

Invoke-PSDocument -Name 'Test';
```

Generates a new Test.md document containing a block quote formatted as a DFM warning.


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
