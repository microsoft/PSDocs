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
- Warning - Inserts a warnding usinf DocFx formatted markdown (DFM)
- Table - Inserts a table from pipeline objects
- Yaml - Inserts a YAML header

### Document

Defines a named block that can be called to output documentation. The document keyword can be defined inline or in a seperate script file.

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
    Section 'Intoduction' {

        # Content of the Introduction section
        'This is a sample document that uses PSDocs keywords to construct a dynamic document.'

        # Define more section content here
    }
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'Sample' -InputObject '';
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


```powershell
# A document definition named Sample
Document 'Sample' {

    # By default each section is included when markdown in generated
    Section 'Included in output' {

        # SEction and section content is included in generated markdown
    }

    # Sections can be optional if the When parameter is specified the expressnio evaluates to $False
    Section 'Not included in output' -When { $False } {

        # Section and section content is not included in generated markdown
    }
}

# Generate markdown from the document definition
Invoke-PSDocument -Name 'Sample' -InputObject '';
```

### Code

You can use the Code statement to generate fenced code sections in markdown. An info string can optionally be specified using the `-Info` parameter.

Syntax:

```text
Code [-Info <String>] [-Body] <ScriptBlock>
```

- `Info` - An info string that can be used to specify the language of the code block.

Examples:

```powershell
Document 'Test' {

    Code {
        powershell.exe -Help
    }
}

Invoke-PSDocument -Name 'Test' -InputObject $Null;
```

Generates a new `Test.md` document containing the `powershell.exe -Help` command line.

```powershell
Document 'Test' {

    Code powershell {
        Get-Item -Path .\;
    }
}

Invoke-PSDocument -Name 'Test' -InputObject $Null;
```

Generates a new `Test.md` document containing script code formatted with the powershell info string.

```powershell
Document 'Test' {

    # Get the output of the Get-Help command
    Get-Help 'Invoke-PSDocument' | Code
}
```

### Note

Creates a block quote formatted as a DocFx Formatted Markdown note.

Syntax:

```text
Note [-Body] <ScriptBlock>
```

Examples:

```powershell
Document 'Test' {

    Note {
        'This is a note.'
    }
}

Invoke-PSDocument -Name 'Test' -InputObject $Null;
```

Generates a new `Test.md` document containing a block quote formatted as a DFM note.

### Warning

Creates a block quote formatted as a DocFx Formatted Markdown warning.

Syntax:

```text
Warning [-Body] <ScriptBlock>
```

Examples:

```powershell
Document 'Test' {

    Warning {
        'This is a warning.'
    }
}

Invoke-PSDocument -Name 'Test' -InputObject $Null;
```

Generates a new `Test.md` document containing a block quote formatted as a DFM warning.

### Table

Creates a formatted table from pipeline objects.

Syntax:

```text
Table [-Property <String[]>]
```

- `-Property` - Filter the table to only the named columns.

Examples:

```powershell
Document 'Test' {

    Section 'Directory list' {

        # Create a row for each child item of C:\
        Get-ChildItem -Path 'C:\' | Table -Property Name,PSIsContainer;
    }
}

Invoke-PSDocument -Name 'Test';
```

Generates a new `Test.md` document containing a table populated with a row for each item. Only the properties Name and PSIsContainer are added as columns.

### Yaml

Creates a yaml header.

Syntax:

```text
Yaml [-Body] <Hashtable>
```

Examples:

```powershell
Document 'Test' {

    Yaml @{
        title = 'An example title'
    }

    'Yaml header may not be rendered by some markdown viewers. See source to view yaml.'
}

Invoke-PSDocument -Name 'Test';
```

Generates a new Test.md document containing a yaml header. An example of the output generated is available [here](/docs/examples/Yaml-header-output.md).

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

```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSDocs/blob/master/docs/keywords/PSDocs/en-US/about_PSDocs_Keywords.md.

## SEE ALSO

- [Invoke-PSDocument](Invoke-PSDocument.md)

## KEYWORDS

- Document
- Section
- Title
- Code
- Note
- Warning
- Table
- Yaml
