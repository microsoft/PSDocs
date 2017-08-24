
# Invoke-DscNodeDocument

## SYNOPSIS
Calls a document definition.

## SYNTAX

```
Invoke-DscNodeDocument -DocumentName <String> [-Path <String>] [-OutputPath <String>]
```

## EXAMPLES

### EXAMPLE 1

```powershell
Document 'Test' {

    Section 'Installed features' {
        'The following Windows features have been installed.'

        $InputObject.ResourceType.WindowsFeature | Table -Property Name,Ensure;
    }
}

Invoke-DscNodeDocument -DocumentName 'Test' -Path '.\nodes' -OutputPath '.\docs';
```

Generates a new markdown document for each node .mof in the path `.\nodes`.
