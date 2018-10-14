#
# Azure Resource Manager documentation definitions
#

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

# Description: A definition to generate markdown for an ARM template
document 'arm-template' {

    # Read JSON files
    $metadata = GetTemplateMetadata -Path docs/scenarios/arm-template/metadata.json;
    $parameters = GetTemplateParameter -Path docs/scenarios/arm-template/template.json;

    # Set document title
    Title $metadata.itemDisplayName

    # Write opening line
    $metadata.Description

    # Add each parameter to a table
    Section 'Parameters' {
        $parameters | Table -Property @{ Name = 'Parameter name'; Expression = { $_.Name }},Description
    }

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
