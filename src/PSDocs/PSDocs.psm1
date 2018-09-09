#
# PSDocs module
#

Set-StrictMode -Version latest;

# Set up some helper variables to make it easier to work with the module
$PSModule = $ExecutionContext.SessionState.Module;
$PSModuleRoot = $PSModule.ModuleBase;

# Import the appropriate nested binary module based on the current PowerShell version
$binModulePath = Join-Path -Path $PSModuleRoot -ChildPath '/desktop/PSDocs.dll';

# if (($PSVersionTable.Keys -contains 'PSEdition') -and ($PSVersionTable.PSEdition -ne 'Desktop')) {
    $binModulePath = Join-Path -Path $PSModuleRoot -ChildPath '/core/PSDocs.dll';
# }

$binaryModule = Import-Module -Name $binModulePath -PassThru;

# When the module is unloaded, remove the nested binary module that was loaded with it
$PSModule.OnRemove = {
    Remove-Module -ModuleInfo $binaryModule;
}

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

        [Parameter(Mandatory = $False)]
        [String[]]$Tag,

        [Parameter(Position = 1, Mandatory = $True)]
        [ScriptBlock]$Body
    )

    process {

        # Write-Verbose -Message "[Document]::BEGIN"

        # Check if document is being defined
        if ($Null -eq (Get-Variable -Name PSDocs -ErrorAction SilentlyContinue)) {
            InitDocumentContext;

            $Script:DocumentBody[$Name] = $Body;

            # Export definition as a function
            Set-Item -Path "function:global:$Name" -Value (${function:GenerateDocumentFn});
        }
        else {
            # Write-Verbose -Message "[Doc] -- Checking document block: $Name";

            if ($PSDocs.Filter.Match($Name, $Tag)) {
                Write-Verbose -Message "[Doc] -- Calling document block: $Name";

                [String[]]$instances = @($InstanceName);

                # If an instance name is not specified, default to the document name
                if ($Null -eq $InstanceName) {
                    $instances = @($Name);
                }

                foreach ($instance in $instances) {

                    # Set the default section level so that sections in the document start from 2
                    $Section = @{ Level = 1; };

                    $document = [PSDocs.Models.ModelHelper]::NewDocument();

                    # Build a path for the document
                    $document.Path = Join-Path -Path $PSDocs.OutputPath -ChildPath "$instance.md";

                    # Define built-in variables
                    [PSVariable[]]$variablesToDefine = @(
                        New-Object -TypeName PSVariable -ArgumentList ('InstanceName', $instance)
                        New-Object -TypeName PSVariable -ArgumentList ('InputObject', $InputObject)
                        New-Object -TypeName PSVariable -ArgumentList ('Parameter', $parameter)
                        New-Object -TypeName PSVariable -ArgumentList ('Section', $Section)
                        New-Object -TypeName PSVariable -ArgumentList ('Document', $document)
                        New-Object -TypeName PSVariable -ArgumentList ('PSDocs', $PSDocs)
                    )

                    $innerResult = $Body.InvokeWithContext($PSDocs.Context.Function, $variablesToDefine) | ConvertToNode;

                    foreach ($r in $innerResult) {
                        $document.Node.Add($r);
                    }

                    Write-Verbose -Message "[Doc] -- Document results [$($document.Node.Count)]";

                    # Create parent path if it doesn't exist
                    $documentParent = Split-Path -Path $document.Path -Parent;

                    if (!(Test-Path -Path $documentParent)) {
                        $Null = New-Item -Path $documentParent -ItemType Directory -Force;
                    }

                    Write-Verbose -Message "[Doc] -- Document output path: $($document.Path)";

                    # Parse the model
                    $PSDocs.WriteDocument($document);
                }
            }
        }

        # Write-Verbose -Message "[Document]::END"
    }
}

# .ExternalHelp PSDocs-Help.xml
function Invoke-PSDocument {

    [CmdletBinding(DefaultParameterSetName = 'Inline')]
    param (
        # The name of the document
        [Parameter(Mandatory = $False, ParameterSetName = 'Path')]
        [Parameter(Position = 0, Mandatory = $True, ParameterSetName = 'Inline')]
        [String[]]$Name,

        [Parameter(Mandatory = $False, ParameterSetName = 'Path')]
        [String[]]$Tag,

        [Parameter(Mandatory = $False)]
        [String[]]$InstanceName,

        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSObject]$InputObject,

        # The path to look for document definitions in
        [Parameter(Position = 0, Mandatory = $True, ParameterSetName = 'Path')]
        [PSDefaultValue(Help = '.')]
        [String]$Path = $PWD,

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

    begin {
        if ($PSCmdlet.ParameterSetName -eq 'Inline') {
            Write-Warning -Message "Invoke-PSDocument with inline document block is depricated."
        }
    }

    process {
        Write-Verbose -Message "[Invoke-PSDocument]::BEGIN";

        $fnParams = $PSBoundParameters;

        GenerateDocumentPath @fnParams;

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
        InvokeTemplate @PSBoundParameters;
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
        [String]$Path = '.\psdocs.yml',

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
function Write-PSDocumentSection {

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
        [ScriptBlock]$When,

        # Optionally create a section block even when it is empty
        [Parameter(Mandatory = $False)]
        [Switch]$Force = $False
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
                $innerResult = $Body.InvokeReturnAsIs($Null) | ConvertToNode;

                foreach ($r in $innerResult) {
                    $result.Node.Add($r);
                }
            }
            catch {

                # Report non-terminating error
                Write-Error -Message ($LocalizedData.SectionProcessFailure -f $_.Exception.Message) -Exception $_.Exception -ErrorId 'PSDocs.Section.ProcessFailure';

                return;
            }

            if ($result.Node.Count -gt 0 -or $Force -or $PSDocs.Option.Markdown.SkipEmptySections -eq $False) {
                # Emit Section object to the pipeline
                $result;
            }
            else {
                Write-Verbose -Message "[Doc][Section] -- Skipped, section is empty";
            }
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
        [Object[]]$Property
    )

    begin {
        Write-Verbose -Message "[Doc][Table] BEGIN::";

        # Create a table
        $table = [PSDocs.Models.ModelHelper]::NewTable();

        $recordIndex = 0;

        $rowData = New-Object -TypeName Collections.Generic.List[Object];
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
        # Extract out the column names based on the resulting objects
        $rowData | ForEach-Object -Process {
            $_.PSObject.Properties
        } | Where-Object -FilterScript {
            $_.IsGettable -and $_.IsInstance
        } | Select-Object -Unique -ExpandProperty Name | ForEach-Object -Process {
            $table.Header.Add([String]$_);
        }

        foreach ($r in $rowData) {

            [String[]]$row = New-Object -TypeName 'String[]' -ArgumentList $table.Header.Count;

            for ($i = 0; $i -lt $row.Length; $i++) {
                $field = GetObjectField -InputObject $r -Field $table.Header[$i] -Verbose:$VerbosePreference;

                if ($Null -ne $field -and $Null -ne $field.Value) {
                    $row[$i] = $field.Value.ToString();
                }
            }

            $table.Rows.Add($row);
        }

        $table;

        Write-Verbose -Message "[Doc][Table] END:: [$($table.Header.Count)]";
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

        if ($Null -eq (Get-Variable -Name DocumentBody -Scope Script -ErrorAction SilentlyContinue)) {
            $Script:DocumentBody = @{ };
        }
    }
}

# Proxy function is used when executing the Document keyword like a function
function GenerateDocumentFn {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $False)]
        [String]$OutputPath = $PWD,

        [Parameter(Mandatory = $False)]
        [String[]]$InstanceName,

        [Parameter(Mandatory = $False)]
        [Switch]$PassThru = $False,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.PSDocumentOption]$Option,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding = [PSDocs.Configuration.MarkdownEncoding]::Default
    )

    process {
        Write-Verbose -Message "[$($MyInvocation.InvocationName)]::BEGIN";

        $fnParams = $PSBoundParameters;

        GenerateDocumentInline -Name $MyInvocation.InvocationName @fnParams;

        Write-Verbose -Message "[$($MyInvocation.InvocationName)]::END";
    }
}

function GenerateDocumentPath {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $False)]
        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [String[]]$Tag,

        [Parameter(Mandatory = $False)]
        [String[]]$InstanceName,

        [Parameter(Mandatory = $False)]
        [PSObject]$InputObject,

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

        # Check if the path is a directory
        if (!(Test-Path -Path $Path)) {
            Write-Error -Message $LocalizedData.PathNotFound -ErrorAction Stop;

            return;
        }

        # Get matching document scripts
        [String[]]$docScripts = (Get-ChildItem -Path $Path -Include '*.doc.ps1' -Recurse -File).FullName;

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] =  $Option;
        }

        if ($PSBoundParameters.ContainsKey('Encoding')) {
            $optionParams['Encoding'] = $Encoding;
        }

        # Get an options object
        $Option = New-PSDocumentOption @optionParams;

        # Create PSDocs variable
        $PSDocs = [PSDocs.Models.PSDocsContext]::Create(
            $Option,
            $Name,
            $Tag
        );

        $PSDocs.WriteDocumentHook = {
                param ([PSDocs.Configuration.PSDocumentOption]$option, [PSDocs.Models.Document]$document)

                Write-Verbose -Message "[zDoc] -- Processing document model";

                # Visit the document with the specified processor
                (NewMarkdownProcessor).Process($option, $document) | WriteDocumentContent -Path $document.Path -PassThru:$PassThru -Encoding:$option.Markdown.Encoding;
        }

        $PSDocs.OutputPath = $OutputPath;

        $PSDocs = Add-Member -PassThru -InputObject $PSDocs -MemberType NoteProperty -Name Context -Value @{
            Function = GetLanguageContext -Function $Function
        }
    }

    process {

        # try {



            # foreach ($instance in $instances) {

                # Write-Verbose -Message "[Doc] -- Processing: $instance";

                # try {

                    foreach ($p in $docScripts) {
                        InvokeTemplate -Path $p -Option $Option -Verbose:$VerbosePreference;
                    }
                # }
                # catch {
                #     # Write-Verbose -Message "Failed to invoke: $($_.Exception.Message)";
                #     Write-Error -Message $LocalizedData.DocumentProcessFailure -Exception $_.Exception -Category OperationStopped -ErrorId 'PSDocs.Document.ProcessFailure';
                # }
            # }
        # }
        # catch {
        #     # ([Exception]$_.Exception).StackTrace[]
        #     Write-Error -Message "Engine error $((Get-PSCallStack)[0].ScriptLineNumber)"
        #     # throw "Engine error: $_";
        # }
    }
}

function GenerateDocumentInline {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $False)]
        [String]$Name,

        [Parameter(Mandatory = $False)]
        [String[]]$InstanceName,

        [Parameter(Mandatory = $False)]
        [PSObject]$InputObject,

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

        $useInline = $False;

        # Check if existing document is inline
        if ($PSBoundParameters.ContainsKey('Name') -and !$PSBoundParameters.ContainsKey('Path')) {

            if ($Null -eq $Script:DocumentBody -or !$Script:DocumentBody.ContainsKey($Name)) {

                Write-Error -Message ($LocalizedData.DocumentNotFound -f $Name) -ErrorAction Stop;

                return;
            }
            else {
                $useInline = $True;
            }
        }

        # Check if the path is a directory
        if (!(Test-Path -Path $Path)) {
            Write-Error -Message $LocalizedData.PathNotFound -ErrorAction Stop;

            return;
        }

        # Get matching document scripts
        [String[]]$docScripts = (Get-ChildItem -Path $Path -Include '*.doc.ps1' -Recurse -File).FullName;

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] =  $Option;
        }

        if ($PSBoundParameters.ContainsKey('Encoding')) {
            $optionParams['Encoding'] = $Encoding;
        }

        # Get an options object
        $Option = New-PSDocumentOption @optionParams;

        # Create PSDocs variable
        # $PSDocs = New-Object -TypeName PSObject -Property @{
        #     Option = $Option;
        # }

        $PSDocs = [PSDocs.Models.PSDocsContext]::Create(
            $Option,
            $Null,
            $Null
        );

        $PSDocs.OutputPath = $OutputPath;

        [Hashtable]$parameter = $Null;

        $functionsToDefine = GetLanguageContext -Function $Function;
    }

    process {

        try {

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

                # Build a path for the document
                $document.Path = Join-Path -Path $OutputPath -ChildPath "$instance.md";

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
                    if ($useInline) {

                        # Invoke the body of the document definition and get the output
                        $body = $Script:DocumentBody[$Name];
                        $innerResult = $body.InvokeWithContext($functionsToDefine, $variablesToDefine) | ConvertToNode;
                    }
                    else {

                        foreach ($p in $docScripts) {
                            $innerResult = InvokeTemplate -Path $p -Verbose:$VerbosePreference;
                        }
                    }
                }
                catch {
                    Write-Verbose -Message "Failed to invoke: $($_.Exception.Message). $($_.Exception.ErroRrecord.ScriptStackTrace)";
                    # Write-Error -Message $LocalizedData.DocumentProcessFailure -Exception $_.Exception -Category OperationStopped -ErrorId 'PSDocs.Document.ProcessFailure' -ErrorAction Stop;
                }

                foreach ($r in $innerResult) {
                    $document.Node.Add($r);
                }

                Write-Verbose -Message "[Doc] -- Document results [$($document.Node.Count)]";

                # Create parent path if it doesn't exist
                $documentParent = Split-Path -Path $document.Path -Parent;

                if (!(Test-Path -Path $documentParent)) {
                    New-Item -Path $documentParent -ItemType Directory -Force | Out-Null;
                }

                # Parse the model
                (NewMarkdownProcessor).Process($option, $document) | WriteDocumentContent -Path $document.Path -PassThru:$PassThru -Encoding:$Option.Markdown.Encoding;
            }
        }
        catch {
            # ([Exception]$_.Exception).StackTrace[]
            # Write-Verbose -Message "Engine error $((Get-PSCallStack).ScriptLineNumber)"
            Write-Verbose -Message "Engine error: $($_.Exception.Message)"
        }
    }
}

function InvokeInline {

    [CmdletBinding()]
    param (
        [String]$Name
    )

    process {
        $Null = $Script:DocumentBody[$Name];
    }
}

function GetLanguageContext {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $False)]
        [AllowNull()]
        [System.Collections.Generic.Dictionary[String, ScriptBlock]]$Function
    )

    process {
        Write-Verbose -Message "[Doc] -- Getting language context";

        # Prepare PSDocs language functions
        $functionsToDefine = New-Object -TypeName 'System.Collections.Generic.Dictionary[String,ScriptBlock]'([System.StringComparer]::OrdinalIgnoreCase);

        # Add external functions
        if ($Null -ne $Function -and $Function.Count -gt 0) {
            foreach ($fn in $Function) {
                $functionsToDefine.Add($fn.Key, $fn.Value);
            }
        }

        # Define built-in functions
        $functionsToDefine['Section'] = ${function:Write-PSDocumentSection};
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

        return $functionsToDefine;
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
        $Null = $stringBuilder.Append($InputObject);
    }

    end {
        if ($PassThru) {
            $stringBuilder.ToString();
        } else {

            Write-Verbose -Message "[Doc] -- Writing to path: $Path";
            [System.IO.File]::WriteAllText($Path, $stringBuilder.ToString(), $contentEncoding);
        }
    }
}

function ParseDom {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [PSDocs.Models.Document]$Document,

        [Parameter(Mandatory = $True)]
        [PSObject]$Processor,

        [Parameter(Mandatory = $True)]
        [PSDocs.Configuration.PSDocumentOption]$Option
    )

    process {

        Write-Verbose -Message "[Doc] -- Processing document model";

        # Visit the document with the specified processor
        $Processor.Visit($Document, $Option);
    }
}

function ConvertToNode {

    [CmdletBinding()]
    [OutputType([PSDocs.Models.DocumentNode])]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        $InputObject
    )

    process {

        if ($InputObject -is [PSDocs.Models.DocumentNode]) {
            return $InputObject;
        }

        if ($InputObject -is [PSObject]) {
            if ($InputObject.PSObject.BaseObject -is [PSDocs.Models.DocumentNode]) {
                return $InputObject.PSObject.BaseObject;
            }
        }

        return [PSDocs.Models.ModelHelper]::Text($InputObject.ToString());
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

    process {
        return New-Object -TypeName PSDocs.Processor.Markdown.MarkdownProcessor;
    }
}

function InvokeTemplate {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.PSDocumentOption]$Option
    )

    process {

        if (!(Test-Path -Path $Path)) {
            Write-Error -Message "Failed to find template" -Category ObjectNotFound;
        }

        Write-Verbose -Message "[Doc] -- Using: $Path"

        try {
            # Create a PS environment for execution
            # A constrained environment will be used if DeviceGuard is enabled
            $runspace = GetRunspace;
            $ps = [powershell]::Create();
            $ps.Runspace = $runspace;

            $ps.AddScript($Path);
            $Null = $ps.Invoke();

            # Replay verbose messages
            $ps.Streams.Verbose | ForEach-Object -Process {
                Write-Verbose -Message $_.Message
            }
        }
        catch {
            Write-Error "failed: $($_.Exception.Message)";
        }
        finally {
            # if ($Null -ne $ps) { $ps.Dispose() }
            # if ($Null -ne $runspace) { $runspace.Dispose() }
        }
    }
}

function GetRunspace {

    process {

        $isDeviceGuard = IsDeviceGuardEnabled;

        # # Define built-in functions
        # $functionsToDefine['Section'] = ${function:Write-PSDocumentSection};
        # $functionsToDefine['Title'] = ${function:Title};
        # $functionsToDefine['List'] = ${function:List};
        # $functionsToDefine['Code'] = ${function:Code};
        # $functionsToDefine['Note'] = ${function:Note};
        # $functionsToDefine['Warning'] = ${function:Warning};
        # $functionsToDefine['Metadata'] = ${function:Metadata};
        # $functionsToDefine['Yaml'] = ${function:Metadata};
        # $functionsToDefine['Table'] = ${function:Table};
        # $functionsToDefine['Format-Table'] = ${function:Table};
        # $functionsToDefine['Format-List'] = ${function:FormatList};

        $iss = [InitialSessionState]::CreateDefault2();
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Document',
            ${function:Document}
        )));
        $iss.Variables.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateVariableEntry -ArgumentList @(
            'PSDocs',
            $PSDocs,
            $Null
        )));
        $iss.Variables.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateVariableEntry -ArgumentList @(
            'VerbosePreference',
            [System.Management.Automation.ActionPreference]::Continue,
            $Null
        )));
        $rs = [RunspaceFactory]::CreateRunspace($iss);
        $rs.Open();

        # If DeviceGuard is enabled, get a contrained execution environment
        if ($isDeviceGuard) {
            $rs.LanguageMode = 'ConstrainedLanguage';
        }

        return $rs;
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

function IsDeviceGuardEnabled
{
    
    if ((Get-Variable -Name IsMacOS -ErrorAction SilentlyContinue) -or (Get-Variable -Name IsLinux -ErrorAction SilentlyContinue))
    {
        return $False;
    }

    # PowerShell 6.0.x does not support Device Guard
    if ($PSVersionTable.PSVersion -ge '6.0' -and $PSVersionTable.PSVersion -lt '6.1')
    {
        return $False;
    }

    return [System.Management.Automation.Security.SystemPolicy]::GetSystemLockdownPolicy() -eq [System.Management.Automation.Security.SystemEnforcementMode]::Enforce;
}


function InitEditorServices {

    [CmdletBinding()]
    param (

    )

    process {

        New-Alias -Name 'Section' -Value Write-PSDocumentSection -Option AllScope -Force -Scope Global;
    }
}

#
# Editor services
#

if ($Null -ne (Get-Variable -Name psEditor -ErrorAction SilentlyContinue)) {
    InitEditorServices;
}

#
# Export module
#

Export-ModuleMember -Function @(
    'Document'
    'Invoke-PSDocument'
    'Get-PSDocumentHeader'
    'New-PSDocumentOption'
    'Write-PSDocumentSection'
);

# EOM