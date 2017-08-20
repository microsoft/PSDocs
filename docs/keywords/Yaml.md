
# Yaml

## SYNOPSIS
Creates a yaml header.

## SYNTAX

```
Yaml [-Body] <Hashtable>
```

## EXAMPLES

### EXAMPLE 1

```powershell
Document 'Test' {

    Yaml @{
        title = 'An example title'
    }
}

Invoke-PSDocument -Name 'Test';
```

Generates a new Test.md document containing a yaml header.


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
