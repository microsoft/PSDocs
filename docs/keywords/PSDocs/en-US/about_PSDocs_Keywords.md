# PSDocs_Keywords

## about_PSDocs_Keywords

## SHORT DESCRIPTION

Describes the language keywords that can be used within PSDocs document definitions.

## LONG DESCRIPTION

PSDocs lets you generate dynamic markdown documents using PowerShell blocks. To generate markdown a document is defined inline or within script files by using the `document` keyword.

- Document - A named document definition
- Section - A named section
- Title - Sets the document title
- Code - Inserts a block of code
- Note - Inserts a note using DocFx formatted markdown (DFM)
- Warning - Inserts a warning using DocFx formatted markdown (DFM)
- Table - Inserts a table from pipeline objects
- Yaml - Inserts a YAML header

### Document

Defines a named block that can be called to output documentation. The document keyword can be defined inline or in a separate script file.

Syntax:

```text
Document [-Name] <String> [-Body] <ScriptBlock>
```

- `Name` - The name of the document definition.
- `Body` - A definition of the markdown document containing one or more PSDocs keywords and PowerShell.

Examples:

```powershell
# A document definition named Sample
Document 'Sample' {

    # Define the document here
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'Sample' -InputObject '';
```

### Section

Creates a new document section block containing content. Each section will be converted into a header.

Syntax:

```text
Section [-Name] <String> [-When <ScriptBlock>] [-Body] <ScriptBlock>
```

- `Name` - The name or header of the section.
- `When` - A condition to determine if the section block should be included in the markdown document.

Examples:

```powershell
# A document definition named Sample
Document 'Sample' {

    # Define a section named Introduction
    Section 'Introduction' {

        # Content of the Introduction section
        'This is a sample document that uses PSDocs keywords to construct a dynamic document.'

        # Define more section content here
    }
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'Sample' -InputObject '';
```

```markdown
## Introduction
This is a sample document that uses PSDocs keywords to construct a dynamic document.
```

```powershell
# A document definition named Sample
Document 'Sample' {

    # Sections can be nested
    Section 'Level2' {

        Section 'Level3' {

            # Define level 3 section content here
        }

        # Define more level 2 section content here
    }
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'Sample' -InputObject '';
```

```markdown
## Level2
### Level3
```

```powershell
# A document definition named Sample
Document 'Sample' {

    # By default each section is included when markdown in generated
    Section 'Included in output' {

        # Section and section content is included in generated markdown
    }

    # Sections can be optional if the When parameter is specified the expression evaluates to $False
    Section 'Not included in output' -When { $False } {

        # Section and section content is not included in generated markdown
    }
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'Sample' -InputObject '';
```

```markdown
## Included in output
```

### Title

You can use the Title statement to set the title of the document.

Syntax:

```text
Title [-Content] <String>
```

- `Content` - Set the title for the document.

Examples:

```powershell
# A document definition named Title
Document 'Title' {

    # Set the title for the document
    Title 'An example document'
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'Title' -InputObject $Null;
```

Generates a new `Title.md` document containing the heading `An example document`.

### Code

You can use the Code statement to generate fenced code sections in markdown. An info string can optionally be specified using the `-Info` parameter.

Syntax:

```text
Code [-Info <String>] [-Body] <ScriptBlock>
```

- `Info` - An info string that can be used to specify the language of the code block.

Examples:

```powershell
# A document definition named CodeBlock
Document 'CodeBlock' {

    # Define a code block that will be rendered as markdown instead of being executed
    Code {
        powershell.exe -Help
    }
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'CodeBlock' -InputObject $Null;
```

Generates a new `CodeBlock.md` document containing the `powershell.exe -Help` command line.

```powershell
# A document definition named CodeBlockWithInfo
Document 'CodeBlockWithInfo' {

    # Define a code block that will be rendered in markdown as PowerShell
    Code powershell {
        Get-Item -Path .\;
    }
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'CodeBlockWithInfo' -InputObject $Null;
```

Generates a new `Test.md` document containing script code formatted with the powershell info string.

```powershell
# A document definition named CodeBlockFromPipeline
Document 'CodeBlockFromPipeline' {

    # Execute Get-Help then create a code block from the output of the Get-Help command
    Get-Help 'Invoke-PSDocument' | Code
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'CodeBlockFromPipeline' -InputObject $Null;
```

### Note

Creates a block quote formatted as a DocFx Formatted Markdown note.

Syntax:

```text
Note [-Body] <ScriptBlock>
```

Examples:

```powershell
# A document definition named NoteBlock
Document 'NoteBlock' {

    # Define a note block
    Note {
        'This is a note.'
    }
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'NoteBlock' -InputObject $Null;
```

```markdown
> [!NOTE]
> This is a note.
```

Generates a new `NoteBlock.md` document containing a block quote formatted as a DFM note.

### Warning

Creates a block quote formatted as a DocFx Formatted Markdown warning.

Syntax:

```text
Warning [-Body] <ScriptBlock>
```

Examples:

```powershell
# A document definition named WarningBlock
Document 'WarningBlock' {

    Warning {
        'This is a warning.'
    }
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'WarningBlock' -InputObject $Null;
```

```markdown
> [!WARNING]
> This is a warning.
```

Generates a new `WarningBlock.md` document containing a block quote formatted as a DFM warning.

### Table

Creates a formatted table from pipeline objects.

Syntax:

```text
Table [-Property <Object[]>]
```

- `-Property` - Filter the table to only the named columns. Either a named column or expression can be used.

Examples:

```powershell
# A document definition named Table
Document 'Table' {

    Section 'Directory list' {

        # Create a row for each child item of C:\
        Get-ChildItem -Path 'C:\' | Table -Property Name,@{ Name = 'IsContainer'; Expression = { $_.PSIsContainer }};
    }
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'Table';
```

```markdown
## Directory list

|Name|IsContainer|
| --- | --- |
|Program Files|True|
|Program Files (x86)|True|
|Users|True|
|Windows|True|
```

Generates a new `Table.md` document containing a table populated with a row for each item. Only the properties Name and IsContainer are added as columns. A property expression is used on the `PSIsContainer` property to render the column as `IsContainer`.

### Metadata

Creates a metadata header, that will be rendered as yaml front matter. Multiple `Metadata` blocks can be used and they will be aggregated together.

Syntax:

```text
Metadata [-Body] <Hashtable>
```

Examples:

```powershell
# A document definition named MetadataBlock
Document 'MetadataBlock' {

    # Create a Metadata block of key value pairs
    Metadata @{
        title = 'An example title'
    }

    Metadata @{
        author = $Env:USERNAME
        'last-updated' = (Get-Date).ToString('yyyy-MM-dd')
    }

    # Additional text to add to the document
    'Yaml header may not be rendered by some markdown viewers. See source to view yaml.'
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'MetadataBlock';
```

Generates a new MetadataBlock.md document containing a yaml front matter. An example of the output generated is available [here](/docs/examples/Yaml-header-output.md).

## EXAMPLES

```powershell

Document 'Sample' {

    Section 'Introduction' {
        'This is a sample document that uses PSDocs keywords to construct a dynamic document.'
    }

    Section 'Generated by' {
        "This document was generated by $($Env:USERNAME)."

       $PSVersionTable | Table -Property Name,Value
    }
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'Sample';
```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSDocs/blob/master/docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md.

## SEE ALSO

- [Invoke-PSDocument](https://github.com/BernieWhite/PSDocs/blob/master/docs/commands/PSDocs/en-US/Invoke-PSDocument.md)

## KEYWORDS

- Document
- Section
- Title
- Code
- Note
- Warning
- Table
- Yaml
