# PSDocs_Variables

## about_PSDocs_Variables

## SHORT DESCRIPTION

Describes the automatic variables that can be used within PSDocs document definitions.

## LONG DESCRIPTION

PSDocs lets you generate dynamic markdown documents using PowerShell blocks. To generate markdown, a document is defined inline or within script files by using the `document` keyword.

Within a document definition, PSDocs exposes a number of automatic variables.

### Culture

The culture of the document currently being processed.

Cultures specified by using the `-Culture` parameter of `Invoke-PSDocument` or inline functions will be sequentially processed. As each is processed, the `$Culture` variable will take on the current processed culture.

If a culture has not been specified, the culture will default to the culture of the current thread.

Syntax:

```powershell
$Culture
```

### InstanceName

The name of the instance currently being processed.

Instance names specified by using the `-InstanceName` parameter of `Invoke-PSDocument` or inline functions will be sequentially processed. As each is processed, the `$InstanceName` variable will take on the currently processed instance name.

If an instance name is not specified, the instance name will default to the name of the document.

Syntax:

```powershell
$InstanceName
```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSDocs/blob/master/docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md.

## SEE ALSO

- [Invoke-PSDocument](https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs/en-US/Invoke-PSDocument.md)

## KEYWORDS

- Culture
- InstanceName
