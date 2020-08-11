# Generate documentation from Desired State Configuration

```powershell
# Import PSDocs.Dsc module
Import-Module -Name PSDocs.Dsc;

# Define a document called Sample
Document 'Sample' {

    # Add an 'Installed features' section in the document
    Section 'Installed features' {
        # Add a comment
        'The following Windows features have been installed.'

        # Generate a table of Windows Features
        $InputObject.ResourceType.WindowsFeature | Table -Property Name,Ensure
    }
}

# Call the document definition and generate markdown for each .mof file in the .\nodes directory
Invoke-DscNodeDocument -DocumentName 'Sample' -Path '.\nodes' -OutputPath '.\docs';
```
