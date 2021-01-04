# Generate a document from a directory listing

```powershell
# File: Sample.Doc.ps1

# Define a document called Sample
Document Sample {

    # Add an introduction section
    Section Introduction {
        # Add a comment
        "This is a sample file list from $TargetObject"

        # Generate a table
        Get-ChildItem -Path $TargetObject | Table -Property Name,PSIsContainer
    }
}
```

To execute the document use `Invoke-PSDocument`.

For example:

```powershell
Invoke-PSDocument -InputObject 'C:\';
```

```powershell
# Import PSDocs module
Import-Module -Name PSDocs;

# Call the document definition as a function to generate markdown from an object
Invoke-PSDocument -InputObject 'C:\';
```

An example of the output generated is available [here](../../examples/Get-child-item-output.md).
