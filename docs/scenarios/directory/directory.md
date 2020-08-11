# Generate a document from a directory listing

```powershell
# Import PSDocs module
Import-Module -Name PSDocs;

# Define a document called Sample
Document Sample {

    # Add an introduction section
    Section Introduction {
        # Add a comment
        "This is a sample file list from $InputObject"

        # Generate a table
        Get-ChildItem -Path $InputObject | Table -Property Name,PSIsContainer
    }
}

# Call the document definition as a function to generate markdown from an object
Sample -InputObject 'C:\';
```

An example of the output generated is available [here](../../examples/Get-child-item-output.md).
