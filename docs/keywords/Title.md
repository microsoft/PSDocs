
# Title

## SYNOPSIS
Sets the title of the document.

## SYNTAX

```
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
