#
# PSDocs module
#

# Import helper classes
if (!$PSVersionTable.PSEdition -or $PSVersionTable.PSEdition -eq 'Desktop') {
    Add-Type -Path (Join-Path -Path $PSScriptRoot -ChildPath "/bin/net451/PSDocs.dll") | Out-Null;
}
# else {
#     Add-Type -Path (Join-Path -Path $PSScriptRoot -ChildPath "/bin/netstandard2.0/PSDocs.dll") | Out-Null;
# }

[PSDocs.Configuration.PSDocumentOption]::GetWorkingPath = {

    return Get-Location;
}

$Script:UTF8_NO_BOM = New-Object -TypeName System.Text.UTF8Encoding -ArgumentList $False;

#
# Localization
#

$LocalizedData = data {
    
}

Import-LocalizedData -BindingVariable LocalizedData -FileName 'PSDocs.Resources.psd1' -ErrorAction SilentlyContinue;

#
# Public functions
#

# .ExternalHelp PSDocs-Help.xml
function Document {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$Name,

        [Parameter(Position = 1, Mandatory = $True)]
        [ScriptBlock]$Body
    )

    process {

        Write-Verbose -Message "[Document]::BEGIN"

        InitDocumentContext;

        $Script:DocumentBody[$Name] = $Body;

        # Export documentation function
        Set-Item -Path "function:global:$Name" -Value (${function:GenerateDocumentFn});

        Write-Verbose -Message "[Document]::END"
    }
}

# .ExternalHelp PSDocs-Help.xml
function Invoke-PSDocument {

    [CmdletBinding()]
    param (
        # The name of the document
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$Name,

        [Parameter(Mandatory = $False)]
        [String[]]$InstanceName,
        
        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSObject]$InputObject,

        # [Parameter(Mandatory = $False)]
        # [Object]$ConfigurationData,

        # The path to look for document definitions in
        # [Parameter(Mandatory = $False)]
        # [String]$Path = $PWD,

        # The output path to save generated documentation
        [Parameter(Mandatory = $False)]
        [String]$OutputPath = $PWD,

        [Parameter(Mandatory = $False)]
        [ValidateNotNull()]
        [System.Collections.Generic.Dictionary[String, ScriptBlock]]$Function,

        [Parameter(Mandatory = $False)]
        [Switch]$PassThru = $False,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.PSDocumentOption]$Option,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding = [PSDocs.Configuration.MarkdownEncoding]::Default
    )

    process {
        Write-Verbose -Message "[Invoke-PSDocument]::BEGIN";

        $fnParams = $PSBoundParameters;

        GenerateDocument @fnParams;
        
        Write-Verbose -Message "[Invoke-PSDocument]::END";
    }
}

# .ExternalHelp PSDocs-Help.xml
function Import-PSDocumentTemplate {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {
        Write-Verbose -Message "[Doc] -- Reading template: $Path";

        ReadTemplate @PSBoundParameters;
    }
}

# .ExternalHelp PSDocs-Help.xml
function Get-PSDocumentHeader {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $False, ValueFromPipelineByPropertyName = $True)]
        [Alias('FullName')]
        [String]$Path = $PWD
    )

    process {

        $filteredItems = Get-ChildItem -Path "$Path\*" -File;

        foreach ($item in $filteredItems) {
            
            ReadYamlHeader -Path $item.FullName -Verbose:$VerbosePreference;
        }

    }
}

# .ExternalHelp PSDocs-Help.xml
function New-PSDocumentOption {

    [CmdletBinding()]
    [OutputType([PSDocs.Configuration.PSDocumentOption])]
    param (
        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.PSDocumentOption]$Option,

        [Parameter(Mandatory = $False)]
        [String]$Path = '.\.psdocs.yml',

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding
    )

    process {

        if ($PSBoundParameters.ContainsKey('Option')) {
            $Option = $Option.Clone();
        }
        elseif ($PSBoundParameters.ContainsKey('Path')) {

            if (!(Test-Path -Path $Path)) {
                
            }

            $Path = Resolve-Path -Path $Path;

            $Option = [PSDocs.Configuration.PSDocumentOption]::FromFile($Path);
        }
        else {
            Write-Verbose -Message "Attempting to read: $Path";

            $Option = [PSDocs.Configuration.PSDocumentOption]::FromFile($Path, $True);
        }

        if ($PSBoundParameters.ContainsKey('Encoding')) {
            $Option.Markdown.Encoding = $Encoding;
        }

        return $Option;
    }
}

#
# Internal language keywords
#

# Implement the Section keyword
function Section {

    [CmdletBinding()]
    [OutputType([PSObject])]
    param (
        # The name of the Section
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$Name,

        # A script block with the body of the Section
        [Parameter(Position = 1, Mandatory = $True)]
        [ScriptBlock]$Body,

        # Optionally a condition that must be met prior to including the Section
        [Parameter(Mandatory = $False)]
        [ScriptBlock]$When
    )

    begin {
        Write-Verbose -Message "[Doc][Section] BEGIN::";
    }

    process {

        $shouldProcess = $True;

        # Evaluate if the Section condition is met
        if ($Null -ne $When) {

            Write-Verbose -Message "[Doc][Section] -- When: $When";

            $conditionResult = $When.InvokeReturnAsIs();

            Write-Verbose -Message "[Doc][Section] -- When: $conditionResult";

            if (($Null -eq $conditionResult) -or ($conditionResult -is [System.Boolean] -and $conditionResult -eq $False)) {
                $shouldProcess = $False;
            }
        }

        # Run Section block if condition was met
        if ($shouldProcess) {
            Write-Verbose -Message "[Doc][Section] -- Adding section: $Name";

            # Create a section
            $result = [PSDocs.Models.ModelHelper]::NewSection($Name, $Section.Level+1);

            # Store as section to be referenced in nested calls
            $Section = $result;

            try {
                # Invoke the Section body and collect the results
                $innerResult = $Body.Invoke();
    
                foreach ($r in $innerResult) {
                    $result.Node += $r;
                }
            }
            catch {
                
                # Report non-terminating error
                Write-Error -Message ($LocalizedData.SectionProcessFailure -f $_.Exception.Message) -Exception $_.Exception -ErrorId 'PSDocs.Section.ProcessFailure';

                return;
            }

            # Emit Section object to the pipeline
            $result;
        }
    }

    end {
        Write-Verbose -Message "[Doc][Section] END::";
    }
}

function Title {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [AllowEmptyString()]
        [String]$Content
    )

    process {
        # Update the document title
        $Document.Title = $Content;
    }
}

function Code {

    [CmdletBinding()]
    [OutputType([PSDocs.Models.Code])]
    param (
        # Body of the code block
        [Parameter(Position = 0, Mandatory = $True, ParameterSetName = 'Default', ValueFromPipeline = $True)]
        [Parameter(Position = 1, Mandatory = $True, ParameterSetName = 'InfoString', ValueFromPipeline = $True)]
        [ScriptBlock]$Body,

        [Parameter(Mandatory = $True, ParameterSetName = 'StringDefault', ValueFromPipeline = $True)]
        [Parameter(Mandatory = $True, ParameterSetName = 'StringInfoString', ValueFromPipeline = $True)]
        [String]$BodyString,

        # Info-string
        [Parameter(Position = 0, Mandatory = $True, ParameterSetName = 'InfoString')]
        [Parameter(Position = 0, Mandatory = $True, ParameterSetName = 'StringInfoString')]
        [String]$Info
    )

    process {

        $result = [PSDocs.Models.ModelHelper]::NewCode();

        if (![String]::IsNullOrWhiteSpace($Info)) {
            $result.Info = $Info.Trim();
        }

        if ($PSCmdlet.ParameterSetName -eq 'StringDefault' -or $PSCmdlet.ParameterSetName -eq 'StringInfoString') {
            $result.Content = $BodyString;
        }
        else {
            $result.Content = $Body.ToString();
        }

        # Cleanup indent

        if ($result.Content -match '^\r\n(?<indent> {1,})') {
            $result.Content = $result.Content -replace "\r\n {1,$($Matches.indent.length)}", '';
        }
        # Check for \n line endings
        elseif ($result.Content -match '^\n(?<indent> {1,})') {
            $result.Content = $result.Content -replace "\n {1,$($Matches.indent.length)}", '';
        }

        $result;
    }
}

function List {

    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [ScriptBlock]$Body
    )

    process {

        $result = New-Object -TypeName PSObject -Property @{ Type = 'List'; Node = @(); };

        $innerResult = $Body.InvokeWithContext($Null, $Null);

        foreach ($r in $innerResult) {
            $result.Node += $r;
        }

        $result;
    }
}

function Note {

    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [ScriptBlock]$Body
    )

    process {

        $result = [PSDocs.Models.ModelHelper]::NewNote();

        $innerResult = $Body.InvokeWithContext($Null, $Null);

        foreach ($r in $innerResult) {
            $result.Content += $r;
        }

        $result;
    }
}

function Warning {

    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [ScriptBlock]$Body
    )

    process {

        $result = [PSDocs.Models.ModelHelper]::NewWarning();

        $innerResult = $Body.InvokeWithContext($Null, $Null);

        foreach ($r in $innerResult) {
            $result.Content += $r;
        }

        $result;
    }
}

function Metadata {

    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [System.Collections.IDictionary]$Body
    )

    process {

        # Process eaxch key value pair in the supplied dictionary/hashtable
        foreach ($kv in $Body.GetEnumerator()) {
            
            $Document.Metadata[$kv.Key] = $kv.Value;
        }
    }
}

function Table {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [AllowNull()]
        [Object]$InputObject,

        [Parameter(Mandatory = $False, Position = 0)]
        [String[]]$Property
    )

    begin {
        Write-Verbose -Message "[Doc][Table] BEGIN::";

        # Create a table
        $table = [PSDocs.Models.ModelHelper]::NewTable();

        $recordIndex = 0;

        $rowData = New-Object -TypeName Collections.Generic.List[Object];

        # if ($Property -is [Hashtable[]]) {
        #     $Property = $Property -as [Hashtable[]];
        # } else {
        #     $Property = $Property -as [String[]];
        # }
    }

    process {

        Write-Verbose -Message "[Doc][Table][$recordIndex] BEGIN::";

        Write-Verbose -Message "[Doc][Table][$recordIndex] -- Adding  '$($InputObject)'";

        if ($Null -ne $InputObject) {
            $selectedObject = Select-Object -InputObject $InputObject -Property $Property;

            $rowData.Add($selectedObject);
        }

        Write-Verbose -Message "[Doc][Table][$recordIndex] END::";

        $recordIndex++;
    }

    end {
        [String[]]$headers = $rowData | ForEach-Object -Process {
            $_.PSObject.Properties
        } | Where-Object -FilterScript {
            $_.IsGettable -and $_.IsInstance
        } | Select-Object -Unique -ExpandProperty Name;

        $table.Header = @($headers);

        foreach ($r in $rowData) {

            $row = New-Object -TypeName 'String[]' -ArgumentList $headers.Length;

            for ($i = 0; $i -lt $row.Length; $i++) {
                $field = GetObjectField -InputObject $r -Field $headers[$i] -Verbose:$VerbosePreference;

                if ($Null -ne $field -and $Null -ne $field.Value) {
                    $row[$i] = $field.Value.ToString();
                }
            }

            $table.Rows.Add($row);
        }

        $table;

        Write-Verbose -Message "[Doc][Table] END:: [$($headers.Length)]";
    }
}

function FormatList {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [Object]$InputObject,

        [Parameter(Mandatory = $False, Position = 0)]
        [String[]]$Property
    )

    begin {
        Write-Verbose -Message "[Doc][FormatList] BEGIN::";

        $recordIndex = 0;
    }

    process {

        Write-Verbose -Message "[Doc][FormatList][$recordIndex] BEGIN::";

        $table = New-Object -TypeName PSObject -Property @{ Type = 'Table'; Header = @($Property); Rows = (New-Object -TypeName Collections.Generic.List[String[]]); ColumnCount = 0; };

        [String[]]$objectFields = @($Property);

        if ($Null -ne $InputObject) {
            
            for ($i = 0; $i -lt $table.Header.Count; $i++) {
                $field = GetObjectField -InputObject $InputObject -Field $objectFields[$i] -Verbose:$VerbosePreference;

                if ($Null -ne $field -and $Null -ne $field.Value) {

                    Write-Verbose -Message "[Doc][FormatList][$recordIndex] -- Adding $($field.Name): $($field.Value)";

                    [String[]]$row = , [String]::Empty * 2;

                    $row[0] = $field.Name;

                    $row[1] = $field.Value;

                    $table.Rows.Add($row);
                }
            }

            $table;
        }

        Write-Verbose -Message "[Doc][FormatList][$recordIndex] END::";

        $recordIndex++;
    }

    end {
        Write-Verbose -Message "[Doc][FormatList] END::";
    }
}

#
# Helper functions
#

function InitDocumentContext {
    [CmdletBinding()]
    param (

    )

    process {

        if ($Null -eq $Script:DocumentBody) {
            $Script:DocumentBody = @{ };
        }
    }
}

function GenerateDocumentFn {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [PSObject]$InputObject,

        # [Parameter(Mandatory = $False)]
        # [Object]$ConfigurationData,

        [Parameter(Mandatory = $False)]
        [String]$OutputPath = $PWD
    )

    process {
        Write-Verbose -Message "[$($MyInvocation.InvocationName)]::BEGIN";

        $fnParams = $PSBoundParameters;

        GenerateDocument -Name $MyInvocation.InvocationName @fnParams;
        
        Write-Verbose -Message "[$($MyInvocation.InvocationName)]::END";
    }
}

function GenerateDocument {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Name,

        [Parameter(Mandatory = $False)]
        [String[]]$InstanceName,

        [Parameter(Mandatory = $False)]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $False)]
        [Object]$ConfigurationData,

        [Parameter(Mandatory = $False)]
        [String]$Path = $PWD,

        [Parameter(Mandatory = $False)]
        [String]$OutputPath = $PWD,

        [Parameter(Mandatory = $False)]
        [System.Collections.Generic.Dictionary[String, ScriptBlock]]$Function,

        [Parameter(Mandatory = $False)]
        [Switch]$PassThru = $False,

        [Parameter(Mandatory = $False)]
        [AllowNull()]
        [PSDocs.Configuration.PSDocumentOption]$Option,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding = [PSDocs.Configuration.MarkdownEncoding]::Default
    )

    begin {
        if ($Null -eq $Script:DocumentBody -or !$Script:DocumentBody.ContainsKey($Name)) {
            
            Write-Error -Message ($LocalizedData.DocumentNotFound -f $Name) -ErrorAction Stop;

            return;
        }

        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] =  $Option;
        }

        if ($PSBoundParameters.ContainsKey('Encoding')) {
            $optionParams['Encoding'] = $Encoding;
        }

        $Option = New-PSDocumentOption @optionParams;

        $PSDocs = New-Object -TypeName PSObject -Property @{
            Option = $Option;
        }

        [Hashtable]$parameter = $Null;

        # Import configuration data from either a hashtable or .psd1 file
        if ($ConfigurationData -is [Hashtable]) {
            $parameter = $ConfigurationData
        } elseif ($ConfigurationData -is [String] -and (Test-Path -Path $ConfigurationData -File)) {
            $parentPath = Split-Path -Parent -Path $ConfigurationData;
            $leafPath = Split-Path -Left -Path $ConfigurationData;

            Import-LocalizedData -BindingVariable parameter -BaseDirectory $parentPath -FileName $leafPath;
        }

        $body = $Script:DocumentBody[$Name];

        # Prepare PSDocs language functions
        $functionsToDefine = New-Object -TypeName 'System.Collections.Generic.Dictionary[String,ScriptBlock]'([System.StringComparer]::OrdinalIgnoreCase);

        # Add external functions
        if ($Null -ne $Function -and $Function.Count -gt 0) {
            foreach ($fn in $Function) {
                $functionsToDefine.Add($fn.Key, $fn.Value);
            }
        }

        # Define built-in functions
        $functionsToDefine['Section'] = ${function:Section};
        $functionsToDefine['Title'] = ${function:Title};
        $functionsToDefine['List'] = ${function:List};
        $functionsToDefine['Code'] = ${function:Code};
        $functionsToDefine['Note'] = ${function:Note};
        $functionsToDefine['Warning'] = ${function:Warning};
        $functionsToDefine['Metadata'] = ${function:Metadata};
        $functionsToDefine['Yaml'] = ${function:Metadata};
        $functionsToDefine['Table'] = ${function:Table};
        $functionsToDefine['Format-Table'] = ${function:Table};
        $functionsToDefine['Format-List'] = ${function:FormatList};
    }

    process {

        [String[]]$instances = @($InstanceName);

        # If an instance name is not specified, default to the document name
        if ($Null -eq $InstanceName) {
            $instances = @($Name);
        }

        # Set the default section level so that sections in the document start from 2
        $Section = @{ Level = 1; };

        foreach ($instance in $instances) {

            Write-Verbose -Message "[Doc] -- Processing: $instance";

            $document = [PSDocs.Models.ModelHelper]::NewDocument();

            # Define built-in variables
            [PSVariable[]]$variablesToDefine = @(
                New-Object -TypeName PSVariable -ArgumentList ('InstanceName', $instance)
                New-Object -TypeName PSVariable -ArgumentList ('InputObject', $InputObject)
                New-Object -TypeName PSVariable -ArgumentList ('Parameter', $parameter)
                New-Object -TypeName PSVariable -ArgumentList ('Section', $Section)
                New-Object -TypeName PSVariable -ArgumentList ('Document', $document)
                New-Object -TypeName PSVariable -ArgumentList ('PSDocs', $PSDocs)
            )

            try {
                # Invoke the body of the document definition and get the output
                $innerResult = $body.InvokeWithContext($functionsToDefine, $variablesToDefine);
            }
            catch {
                Write-Error -Message $LocalizedData.DocumentProcessFailure -Exception $_.Exception -Category OperationStopped -ErrorId 'PSDocs.Document.ProcessFailure' -ErrorAction Stop;
            }

            $innerResult.Insert(0, $document);

            # Create a document object model based on the output
            $dom = New-Object -TypeName PSObject -Property @{ Node = $innerResult; };
            
            # Build a path for the document
            $documentPath = Join-Path -Path $OutputPath -ChildPath "$instance.md";

            # Create parent path if it doesn't exist
            $documentParent = Split-Path -Path $documentPath -Parent;

            if (!(Test-Path -Path $documentParent)) {
                New-Item -Path $documentParent -ItemType Directory -Force | Out-Null;
            }

            # Parse the model
            ParseDom -Dom $dom -Processor (NewMarkdownProcessor) -Option $Option -Verbose:$VerbosePreference | WriteDocumentContent -Path $documentPath -PassThru:$PassThru -Encoding:$Option.Markdown.Encoding;
        }
    }
}

function WriteDocumentContent {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [PSObject]$InputObject,

        # The path to the document.
        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $False)]
        [Switch]$PassThru = $False,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding
    )

    begin {
        $stringBuilder = New-Object -TypeName System.Text.StringBuilder;

        $contentEncoding = $Script:UTF8_NO_BOM;

        switch ($Encoding) {
            'UTF8' { $contentEncoding = [System.Text.Encoding]::GetEncoding('UTF-8'); break; }
            'UTF7' { $contentEncoding = [System.Text.Encoding]::GetEncoding('UTF-7'); break; }
            'Unicode' { $contentEncoding = [System.Text.Encoding]::GetEncoding('Unicode'); break; }
            'UTF32' { $contentEncoding = [System.Text.Encoding]::GetEncoding('UTF-32'); break; }
            'ASCII' { $contentEncoding = [System.Text.Encoding]::GetEncoding('ASCII'); break; }
        }
    }

    process {
        $stringBuilder.AppendLine($InputObject);
    }

    end {
        if ($PassThru) {
            $stringBuilder.ToString();
        } else {
            [System.IO.File]::WriteAllText($Path, $stringBuilder.ToString(), $contentEncoding);
        }
        
    }
}

function ParseDom {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [PSObject]$Dom,

        [Parameter(Mandatory = $True)]
        [PSObject]$Processor,

        [Parameter(Mandatory = $True)]
        [PSDocs.Configuration.PSDocumentOption]$Option
    )

    process {

        $nodeCounter = 0;

        # Process each node of the DOM
        $innerResult = $Dom.Node | ForEach-Object -Process {
            $node = $_;

            Write-Verbose -Message "[Doc][ParseDom] -- Processing node [$nodeCounter]";

            if ($Null -ne $node) {

                # Visit the node
                $Processor.Visit($node, $Option);
            }

            $nodeCounter++;
        }

        $innerResult;
    }
}

function HasProperty {
    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param (
        [PSObject]$InputObject,

        [Parameter(Mandatory = $True)]
        [String]$Name
    )

    process {
        return $Null -ne ($InputObject.PSObject.Properties | Where-Object -FilterScript { $_.Name -eq $Name });
    }
}

function GetObjectField {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $True)]
        [String]$Field
    )

    process {
        # Split field into dotted notation
        $fieldParts = $Field.Split('.');

        if ($Null -eq $InputObject) {
            Write-Error -Message "Failed to bind to InputObject"

            return;
        }

        Write-Verbose -Message "[GetObjectField] -- Getting field: $Field";

        Write-Debug -Message "[GetObjectField] - Splitting into fields: $([String]::Join(',', $fieldParts))";

        # Write-Verbose -Message "[Get-ObjectField] - Detecting type as $($InputObject.GetType())";

        $resultProperty = $Null;

        $nextObj = $InputObject;
        $partIndex = 0;

        $resultPropertyPath = New-Object -TypeName 'System.Collections.Generic.List[String]';

        while ($Null -ne $nextObj -and $partIndex -lt $fieldParts.Length -and $Null -eq $resultProperty) {

            Write-Debug -Message "[GetObjectField] - Checking field part $($fieldParts[$partIndex])";

            # Find a property of the object that matches the current field part

            $property = $Null;

            if ($nextObj -is [System.Collections.Hashtable]) {
                # Handle hash table

                $property = $nextObj.GetEnumerator() | Where-Object `
                -FilterScript {
                    $_.Name -eq $fieldParts[$partIndex]
                }
            } elseif ($nextObj -is [PSObject]) {
                # Handle regular object

                $property = $nextObj.PSObject.Properties.GetEnumerator() | Where-Object `
                -FilterScript {
                    $_.Name -eq $fieldParts[$partIndex]
                }
            }

            if ($Null -ne $property -and $partIndex -eq ($fieldParts.Length - 1)) {
                # We have reached the last field part and found a property

                # Build the remaining field path
                $resultPropertyPath.Add($property.Name);

                # Create a result property object
                $resultProperty = New-Object -TypeName PSObject -Property @{ Name = $property.Name; Value = $property.Value; Path = [String]::Join('.', $resultPropertyPath); };
            } else {
                $nextObj = $property.Value;

                $resultPropertyPath.Add($property.Name);

                $partIndex++;
            }
        }

        # Return the result property
        return $resultProperty;
    }
}

function NewMarkdownProcessor {

    [CmdletBinding()]
    param (

    )

    process {
        # Create an instanced of a markdown processor from an external module
        $result = Import-Module $PSScriptRoot\PSDocsProcessor\Markdown -AsCustomObject -PassThru -Verbose:$False;

        # Return the processor
        $result;
    }
}

function ReadTemplate {
    
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {

        # Read the contents of a .ps1 file
        $template = Get-Content -Path $Path -Raw;

        # Invoke the contents of the .ps1 file as a script block
        $templateScriptBlock = [ScriptBlock]::Create($template);
        $templateScriptBlock.Invoke();
    }
}

function ReadYamlHeader {

    [CmdletBinding()]
    [OutputType([System.Collections.Hashtable])]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {

        # Read the file
        $content = Get-Content -Path $Path -Raw;

        # Detect Yaml header
        if (![String]::IsNullOrEmpty($content) -and $content -match '^(---\r\n(?<yaml>([A-Z0-9]{1,}:[A-Z0-9 ]{1,}(\r\n){0,}){1,})\r\n---\r\n)') {

            Write-Verbose -Message "[Doc][Toc]`t-- Reading Yaml header: $Path";

            # Extract yaml header key value pair
            [String[]]$yamlHeader = $Matches.yaml -split "`n";

            $result = @{ };

            # Read key values into hashtable
            foreach ($item in $yamlHeader) {
                $kv = $item.Split(':', 2, [System.StringSplitOptions]::RemoveEmptyEntries);

                Write-Debug -Message "Found yaml keypair from: $item";

                if ($kv.Length -eq 2) {
                    $result[$kv[0].Trim()] = $kv[1].Trim();
                }
            }

            # Emit result to the pipeline
            return $result;
        }
    }
}

#
# Export module
#

Export-ModuleMember -Function @(
    'Document'
    'Invoke-PSDocument'
    'Import-PSDocumentTemplate'
    'Get-PSDocumentHeader'
    'New-PSDocumentOption'
);

# EOM