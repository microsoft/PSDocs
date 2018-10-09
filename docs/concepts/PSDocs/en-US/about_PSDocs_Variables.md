# PSDocs_Variables

## about_PSDocs_Variables

## SHORT DESCRIPTION

Describes the automatic variables that can be used within PSDocs document definitions.

## LONG DESCRIPTION

PSDocs lets you generate dynamic markdown documents using PowerShell blocks. To generate markdown, a document is defined inline or within script files by using the `document` keyword.

Within a document definition, PSDocs exposes a number of automatic variables that can be read to assist with dynamic document generation.

The following variables are available for use:

- [$Culture](#culture)
- [$Document](#document)
- [$InstanceName](#instancename)
- [$InputObject](#inputobject)
- [$Section](#section)

### Culture

The name of the culture currently being processed.

Cultures specified by using the `-Culture` parameter of `Invoke-PSDocument` or inline functions will be sequentially processed. As each is processed, the `$Culture` variable will take on the current processed culture.

If a culture has not been specified, the culture will default to the culture of the current thread.

Syntax:

```powershell
$Culture
```

### Document

### InstanceName

The name of the instance currently being processed.

Instance names specified by using the `-InstanceName` parameter of `Invoke-PSDocument` or inline functions will be sequentially processed. As each is processed, the `$InstanceName` variable will take on the currently processed instance name.

If an instance name is not specified, the instance name will default to the name of the document.

Syntax:

```powershell
$InstanceName
```

### InputObject

The value of the pipeline object currently being processed. `$InputObject` is set by using the `-InputObject` parameter of `Invoke-PSDocument` or inline functions.

When more than one `$InputObject` is set, each object will be processed sequentially.

Syntax:

```powershell
$InputObject
```

### Section

An object of the document section currently being processed.

As `Section` blocks are processed, the `$Section` variable will be updated to match the block that is currently being processed. `$Section` will be the current document outside of `Section` blocks.

The following properties are available for the section node:

- `Title` - The title of the section, or the document (when outside of a section block).
- `Level` - The section heading depth. This will be `2` (or greater for nested sections), or 1 (when outside of a section block).

Syntax:

```powershell
$Section
```

Examples:

```powershell
document 'Sample' {
    Section 'Introduction' {
        # The value of $Section.Title = 'Introduction'
        "The current title is $($Section.Title)."
    }
}
```

```text
## Introduction

The current section title is Introduction.
```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSDocs/blob/master/docs/concepts/PSDocs/en-US/about_PSDocs_Variables.md.

## SEE ALSO

- [Invoke-PSDocument](https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs/en-US/Invoke-PSDocument.md)

## KEYWORDS

- Culture
- Document
- InstanceName
- InputObject
- Section
