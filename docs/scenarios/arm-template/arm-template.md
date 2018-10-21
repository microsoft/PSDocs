# Azure Resource Manager template example

This is an example of how PSDocs can be used to generate documentation for an ARM template. Documentation for ARM templates might be used by an internal technical team, who creates and maintains ARM templates for their company.

In this scenario we will use two JSON files:

- `template.json` - A valid ARM template
- `metadata.json` - Contains information about the template that isn't part of the ARM template specification

> Our `metadata.json` uses the same schema used in the Azure Quick Start Templates GitHub repository.

When deploying an ARM template, knowing what parameters are available and how they can be used is important, so this will be a key part of our documentation.

Fortunately, the ARM template specification allows for metadata per parameter, and a common use for this is to define a parameter description.

An example parameter might look like this:

```json
"environment": {
    "type": "string",
    "metadata": {
        "description": "The environment that the resource will be deployed to. Either production or internal."
    }
}
```

## Define helper functions

We will need to import our two JSON files and convert them to objects so that we can easily read the name of each parameter, but also the description.

While this could be done inline, we will create separate functions that can be called as required. Using separate functions in this case will improve the readability of our code.

```powershell
# A function to break out parameters from an ARM template
function GetTemplateParameter {
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {
        $template = Get-Content $Path | ConvertFrom-Json;
        foreach ($property in $template.parameters.PSObject.Properties) {
            [PSCustomObject]@{
                Name = $property.Name
                Description = $property.Value.metadata.description
            }
        }
    }
}

# A function to import metadata
function GetTemplateMetadata {
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {
        $metadata = Get-Content $Path | ConvertFrom-Json;
        return $metadata;
    }
}
```

## Create a document definition

PSDocs uses the `document` keyword to describe a document definition. A document definition is designed to be reusable.

With our two helper functions already implemented, we are ready to define our document. For our example, our JSON files are in the same directory as the documentation definition so we are using `$PSScriptRoot`.

```powershell
document 'arm-template' {

    # Read JSON files
    $metadata = GetTemplateMetadata -Path $PSScriptRoot/metadata.json;
    $parameters = GetTemplateParameter -Path $PSScriptRoot/template.json;
}
```

We want to set a title and an opening description for our document based on the metadata file.

```powershell
document 'arm-template' {

    # Read JSON files
    $metadata = GetTemplateMetadata -Path $PSScriptRoot/metadata.json;
    $parameters = GetTemplateParameter -Path $PSScriptRoot/template.json;

    # Set document title
    Title $metadata.itemDisplayName

    # Write opening line
    $metadata.Description
}
```

Next we need to output the template parameters into a table with metadata descriptions. To format our parameters in a table we use the `Table` keyword.

```powershell
document 'arm-template' {

    # Read JSON files
    $metadata = GetTemplateMetadata -Path $PSScriptRoot/metadata.json;
    $parameters = GetTemplateParameter -Path $PSScriptRoot/template.json;

    ...

    # Add each parameter to a table
    Section 'Parameters' {
        $parameters | Table -Property @{ Name = 'Parameter name'; Expression = { $_.Name }},Description
    }
}
```

We can also provide an example command line that can be used to deploy our ARM template. To insert a code sample use the `Code` keyword.

```powershell
document 'arm-template' {

    ...

    # Generate example command line
    Section 'Use the template' {
        Section 'PowerShell' {
            'New-AzureRmResourceGroupDeployment -Name <deployment-name> -ResourceGroupName <resource-group-name> -TemplateFile <path-to-template>' | Code powershell
        }

        Section 'Azure CLI' {
            'az group deployment create <resource-group-name> <deployment-name> --template-file <path-to-template>' | Code text
        }
    }
}
```

## Generate markdown

Document definitions can be called inline or from a path. In this example, we've saved our definition to a file.

To generate markdown from a path, we used the `Invoke-PSDocument` cmdlet with the `-Path` parameter.

Examples:

```powershell
# Find and build any document definitions in the currently working path (and subdirectories)
Invoke-PSDocument -Path .;
```

In this case, we are generating documentation with the definition and output saved in this repository so we use the`-OutputPath` and `-InstanceName` parameters.

```powershell
# Generate docs/scenarios/arm-template/output.md
Invoke-PSDocument -Path '.\docs\scenarios\arm-template' -OutputPath '.\docs\scenarios\arm-template\' -InstanceName 'output';
```

## More information

- [Get the full script](arm-template.doc.ps1)
- [Example ARM template file](template.json)
- [Example ARM metadata file](metadata.json)
- [Example output](output.md)
