# PSDocs_Variables

## about_PSDocs_Variables

## SHORT DESCRIPTION

Describes the automatic variables that can be used within PSDocs document definitions.

## LONG DESCRIPTION

PSDocs lets you generate dynamic markdown documents using PowerShell blocks.
To generate markdown, a document is defined inline or within script files by using the `document` keyword.

Within a document definition, PSDocs exposes a number of automatic variables that can be read to assist with dynamic document generation.
Overwriting these variables or variable properties is not supported.

The following variables are available for use:

- [$Culture](#culture)
- [$Document](#document)
- [$InstanceName](#instancename)
- [$InputObject](#inputobject)
- [$Section](#section)

### Culture

The name of the culture currently being processed.
`$Culture` is set by using the `-Culture` parameter of `Invoke-PSDocument` or inline functions.

When more than one culture is set, each will be processed sequentially.
If a culture has not been specified, `$Culture` will default to the culture of the current thread.

Syntax:

```powershell
$Culture
```

### Document

An object representing the current object model of the document during generation.

The following section properties are available for public read access:

- `Title` - The title of the document.
- `Metadata` - A dictionary of metadata key/value pairs.
- `Path` - The file path where the document will be written to.

Syntax:

```powershell
$Document
```

Examples:

```powershell
document 'Sample' {
    Title 'Example'

    # The value of $Document.Title = 'Example'
    "The title of the document is $($Document.Title)."

    Metadata @{
        author = 'Bernie'
    }

    # The value of $Document.Metadata['author'] = 'Bernie'
    'The author is ' + $Document.Metadata['author'] + '.'
}
```

```text
---
author: Bernie
---
# Example
The title of the document is Example.
The author is Bernie.
```

### InstanceName

The name of the instance currently being processed.
`$InstanceName` is set by using the `-InstanceName` parameter of `Invoke-PSDocument` or inline functions.

When more than one instance name is set, each will be processed sequentially.
If an instance name is not specified, `$InstanceName` will default to the name of the document definition.

Syntax:

```powershell
$InstanceName
```

### InputObject

The value of the pipeline object currently being processed.
`$InputObject` is set by using the `-InputObject` parameter of `Invoke-PSDocument` or inline functions.

When more than one input object is set, each object will be processed sequentially.
If an input object is not specified, `$InputObject` will default to `$Null`.

Syntax:

```powershell
$InputObject
```

### Section

An object of the document section currently being processed.

As `Section` blocks are processed, the `$Section` variable will be updated to match the block that is currently being processed.
`$Section` will be the current document outside of `Section` blocks.

The following section properties are available for public read access:

- `Title` - The title of the section, or the document (when outside of a section block).
- `Level` - The section heading depth. This will be _2_ (or greater for nested sections), or _1_ (when outside of a section block).

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
