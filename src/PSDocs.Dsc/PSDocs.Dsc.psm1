#
# PSDocs DSC extensions module
#

Set-StrictMode -Version latest;

class DscMofDocument {

    [System.Collections.Generic.Dictionary[String, PSObject[]]]$ResourceType

    [System.Collections.Generic.Dictionary[String, PSObject]]$ResourceId

    [String]$Path

    [String]$InstanceName

    DscMofDocument() {
        $this.ResourceId = @{ };
        $this.ResourceType = @{ };
    }
}

#
# Localization
#

$LocalizedData = data {
    
}

Import-LocalizedData -BindingVariable LocalizedData -FileName 'PSDocs.Dsc.Resources.psd1' -ErrorAction SilentlyContinue;

#
# Public functions
#

# .ExternalHelp PSDocs.Dsc-Help.xml
function Invoke-DscNodeDocument {

    [CmdletBinding()]
    param (
        # The name of the document
        [Parameter(Mandatory = $False)]
        [String]$DocumentName,
        
        # A script or path to the script to run
        [Parameter(Mandatory = $False)]
        [String]$Script,

        [Parameter(Mandatory = $False)]
        [String[]]$InstanceName,

        # The path to the .mof files
        [Parameter(Mandatory = $False)]
        [String]$Path = $PWD,

        # The path to output documentation
        [Parameter(Mandatory = $False)]
        [String]$OutputPath = $PWD,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding = [PSDocs.Configuration.MarkdownEncoding]::Default
    )

    begin {
        Write-Verbose -Message "[Invoke-DscNodeDocument]::BEGIN";
    }

    process {
        # Build the documentation
        BuildDocumentation @PSBoundParameters;
    }

    end {
        Write-Verbose -Message "[Invoke-DscNodeDocument]::END";
    }
}

# .ExternalHelp PSDocs.Dsc-Help.xml
function Get-DscMofDocument {

    [CmdletBinding()]
    [OutputType([DscMofDocument])]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {
        # Import and return the .mof as an object containing resource instances
        ImportMofDocument -Path $Path -Verbose:$VerbosePreference;
    }
}

#
# Helper functions
#

function BuildDocumentation {

    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $False)]
        [String]$DocumentName,

        [Parameter(Mandatory = $False)]
        [String]$Script,

        [Parameter(Mandatory = $False)]
        [String[]]$InstanceName,

        # The path to the .mof file
        [Parameter(Mandatory = $False)]
        [String]$Path = $PWD,

        # The output path to store documentaion
        [Parameter(Mandatory = $False)]
        [String]$OutputPath = $PWD,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding
    )

    process {

        if (!(Test-Path -Path $Path)) {
            throw (New-Object -TypeName System.IO.DirectoryNotFoundException);
        }

        $Path = Resolve-Path -Path $Path;

        $referenceConfig = New-Object -TypeName System.Collections.Generic.List[PSObject];

        try {
            # Look for .mof file within the path
            $referenceConfigFilePath = FindMofDocument -Path $Path -InstanceName $InstanceName;

            if ($Null -eq $referenceConfigFilePath -or $referenceConfigFilePath.Length -le 0) {
                return;
            }

            # Extract a reference configuration for each .mof file
            foreach ($file in $referenceConfigFilePath) {
                $referenceConfig.Add((ImportMofDocument -Path $file -Verbose:$VerbosePreference));
            }
        }
        catch {
            Write-Error -Message ($LocalizedData.ImportMofFailed -f $Path, $_.Exception.Message) -Exception $_.Exception -ErrorAction Stop;
        }

        foreach ($r in $referenceConfig) {
            Write-Verbose -Message "[Doc][Mof] -- Using: $($r.Path)";

            $invokeParams = @{
                InstanceName = $r.InstanceName
                InputObject = $r
                OutputPath = $OutputPath
            }

            if ($PSBoundParameters.ContainsKey('Encoding')) {
                $invokeParams['Encoding'] = $Encoding;
            }

            if ($PSBoundParameters.ContainsKey('Script')) {
                $invokeParams['Path'] = $Script;

                Invoke-PSDocument @invokeParams -Verbose:$VerbosePreference;
            }
            else {
                $documentFn = Get-ChildItem -Path "Function:\$DocumentName" -ErrorAction Ignore;

                if ($Null -eq $documentFn) {
                    Write-Error "Failed for find document";

                    continue;
                }

                # Generate a document for the configuration
                [ScriptBlock]::Create([String]::Concat($DocumentName,' @invokeParams -Verbose:$VerbosePreference;')).Invoke();
            }
        }

        # Write-Verbose -Message "[Doc][$dokOperation] -- Update TOC: $($buildResult.FullName)";

        # Update TOC
        # UpdateToc -OutputPath $OutputPath -Verbose:$VerbosePreference;
    }
}

# Builds a configuration graph from a .mof file
function ImportMofDocument {

    [CmdletBinding()]
    [OutputType([DscMofDocument])]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {

        Write-Verbose -Message "[Doc][Mof][Import]::BEGIN";

        # Parse a .mof file and extract object instances
        $instances = ParseMofDocument -Path $Path -Verbose:$VerbosePreference;

        # Extract the instance name from the .mof file name
        $Path -match '\\((?<name>[A-Z0-9_]{3,})(\.meta){0,}\.mof)$' | Out-Null;
        $instanceName = $Matches.name;

        # Build a configuration object
        $result = New-Object -TypeName DscMofDocument -Property @{
            InstanceName = $instanceName
            Path = $path
        };

        # Process each instance and inde by id and type
        foreach ($instance in $instances) {

            $resourceId = $instance.ResourceId;
            $resourceType = $Null;

            if (![String]::IsNullOrEmpty($resourceId)) {

                # Extract resource type from ResourceId
                if ($resourceId -match '^(\s{0,}\[(?<type>[A-Z0-9_:]*)\][A-Z0-9_:\]\[]*)$') {
                    $resourceType = $Matches.type;
                }

                Write-Verbose -Message "[Doc][Mof][Import] -- Adding resource id: $resourceId";
                
                # Add the instance indexed by ResourceId
                $result.ResourceId[$resourceId] = $instance;
                
                if ($Null -ne $resourceType) {
                    if (!$result.ResourceType.ContainsKey($resourceType)) {
                        Write-Verbose -Message "[Doc][Mof][Import] -- Adding resource type: $resourceType";

                        $result.ResourceType.Add($resourceType, @());
                    }
                    
                    # Add the instance indexed by ResourceType
                    $result.ResourceType[$resourceType] += $instance;
                }
            }
        }

        # Emit the mof graph object to the pipeline
        $result;

        Write-Verbose -Message "[Doc][Mof][Import]::END";
    }
}

# Parses a .mof file into object insances
function ParseMofDocument {
    [CmdletBinding()]
    param (
        # The path to the .mof file
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {

        Write-Verbose -Message "[Doc][Mof][Import] -- Parsing: $Path";

        # Split the .mof into instances
        $instances = ((Get-Content $Path -Raw) -split "\n(?=instance of)" -match 'instance of ([A-Z_]*) as');

        # This variable will store configuration items
        $result = New-Object -TypeName System.Collections.Generic.List[PSObject];

        # Process each instance
        foreach ($instance in $instances) {

            # This variable will store properties for a single configuration item
            $props = New-Object -TypeName 'System.Collections.Generic.Dictionary[String,Object]'([System.StringComparer]::OrdinalIgnoreCase);

            # Extract out properties from mof instance block
            $instance -match '\n\{(\r|\n)(?<props>(.|\n)+)\};' | Out-Null;

            # Cleanup new line, line feeds and space padding
            $inner = ($Matches.props -replace '(\r|\n){1,}\s{1,}', "`n") -replace "\n\r", "`n" -split ";\n";

            # Process each property for the configuration item
            $inner | ForEach-Object -Process {

                # Break out key value pairs
                $prop = ($_ -replace '\r|\n', '') -Split '\s{0,}=\s{0,}',2;

                # Ensure that a key value pair was found
                if ($prop.Length -eq 2) {
                    
                    # Cleanup value by removing quotes, line feeds and escaped slashes
                    $value = ($prop[1] -replace '^(\")|(\"(\;\r){0,})$', '') -replace '\\\\', '\';

                    # Look for array values
                    if ($value -match '^(\{(?<array>.*)\})$') {

                        $valueArray = $Matches.array;

                        # Look for string array for type convertion
                        if ($valueArray -match '(^\"|\"$)') {

                            # Force value to be a string array, and cleanup quotes
                            $value = [String[]]@(($valueArray -split '","' -replace '(^\"|\"$)', ''));
                        } else {

                            # Convert value to object array
                            $value = $valueArray -split ',';
                        }
                    }

                    # Add key value pair to dictionary
                    $props[$prop[0]] = $value;
                }
            }

            # Add object based on properties to result
            $result.Add((New-Object -TypeName PSObject -Property $props));
        }

        # Emit result to the pipeline
        $result;

        Write-Verbose -Message "[Doc][Mof][Import] -- Found instances: $($result.Count)";
    }
}

# Finds .mof file in a specified path
function FindMofDocument {

    [CmdletBinding()]
    [OutputType([String])]
    param (
        # The directory path to search for .mof files within
        [Parameter(Mandatory = $True)]
        [String]$Path,

        # An optional InstanceName filter to filter .mof files returned
        [Parameter(Mandatory = $False)]
        [String[]]$InstanceName
    )

    process {
        Write-Verbose -Message "[Doc][Mof] -- Scanning for .mof files in: $Path";

        # Search for mof files
        $items = Get-ChildItem -Path $Path -Filter *.mof -File;

        foreach ($item in $items) {
            if ($Null -eq $InstanceName -or $InstanceName -contains $item.BaseName) {
                # Emit the full name of a mof file to the pipeline when it matches the criteria
                $item.FullName;
            }
        }
    }
}

#
# Export module
#

Export-ModuleMember -Function @(
    'Invoke-DscNodeDocument'
    'Get-DscMofDocument'
)

# EOM