#
# PSDocs module
#

Set-StrictMode -Version latest;

# Set up some helper variables to make it easier to work with the module
$PSModule = $ExecutionContext.SessionState.Module;
$PSModuleRoot = $PSModule.ModuleBase;

# Import the appropriate nested binary module based on the current PowerShell version
$binModulePath = Join-Path -Path $PSModuleRoot -ChildPath '/desktop/PSDocs.dll';

if (($PSVersionTable.Keys -contains 'PSEdition') -and ($PSVersionTable.PSEdition -ne 'Desktop')) {
    $binModulePath = Join-Path -Path $PSModuleRoot -ChildPath '/core/PSDocs.dll';
}

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

#region Cmdlets

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
        # Check if document is being defined
        if ($Null -eq (Get-Variable -Name PSDocs -ErrorAction Ignore)) {
            Write-Verbose -Message "[Doc] -- Exporting definition: $Name";

            InitDocumentContext;

            $Script:DocumentBody[$Name] = $Body;

            # Export definition as a function
            Set-Item -Path "function:global:$Name" -Value (${function:GenerateDocumentFn});
        }
        else {
            # Check that the filter matches before processing
            if ($PSDocs.Filter.Match($Name, $Tag)) {
                Write-Verbose -Message "[Doc] -- Calling document block: $Name";

                [String[]]$instances = @($PSDocs.InstanceName);
                [String[]]$cultures = @($PSDocs.Culture);

                # If an instance name is not specified, default to the document name
                if ($Null -eq $PSDocs.InstanceName) {
                    $instances = @($Name);
                }

                if ($Null -eq $PSDocs.Culture) {
                    $cultures = @([System.Threading.Thread]::CurrentThread.CurrentCulture.Name);
                }

                Write-Verbose -Message "[Doc] -- Will process instances: $($instances.Length)";

                foreach ($instance in $instances) {

                    Write-Verbose -Message "[Doc] -- Processing instance: $instance";

                    foreach ($cultureName in $cultures) {

                        Write-Verbose -Message "[Doc] -- Using culture: $cultureName";

                        # Define scope variables
                        $document = [PSDocs.Models.ModelHelper]::NewDocument();
                        Set-Variable -Name Section -Value $document;
                        Set-Variable -Name InstanceName -Value $instance;
                        Set-Variable -Name Culture -Value $cultureName;

                        # Build a path for the document
                        if ($Null -eq $PSDocs.Culture) {
                            $document.Path = Join-Path -Path $PSDocs.OutputPath -ChildPath "$instance.md";
                        }
                        else {
                            $culturePath = Join-Path -Path $PSDocs.OutputPath -ChildPath $cultureName;
                            $document.Path = Join-Path -Path $culturePath -ChildPath "$instance.md";
                        }

                        $innerResult = $Body.Invoke() | ConvertToNode;

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
        }
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
        [Switch]$PassThru = $False,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.PSDocumentOption]$Option,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding = [PSDocs.Configuration.MarkdownEncoding]::Default,

        [Parameter(Mandatory = $False)]
        [String[]]$Culture
    )

    begin {
        if ($PSCmdlet.ParameterSetName -eq 'Inline') {
            Write-Warning -Message "Invoke-PSDocument with inline document block is depricated."
        }
    }

    process {
        Write-Verbose -Message "[Invoke-PSDocument]::BEGIN";

        $invokeParams = $PSBoundParameters;

        try {
            if ($PSCmdlet.ParameterSetName -eq 'Inline') {
                foreach ($n in $Name) {
                    Write-Verbose -Message "[Invoke-PSDocument] -- Calling by name: $n";

                    $invokeParams['Name'] = $n;
                    GenerateDocumentInline @invokeParams;
                }
            }
            else {
                GenerateDocumentPath @invokeParams;
            }
        }
        catch {
            # Handle exceptions to provide meaningful errors

            $ex = $_;

            if ($Null -ne $ex.Exception.InnerException) {

                $errorParams = @{
                    Exception = $ex.Exception
                    Category = [System.Management.Automation.ErrorCategory]::OperationStopped
                }

                if ($ex.Exception.InnerException -is [System.Management.Automation.IContainsErrorRecord]) {
                    $errorParams['ErrorId'] = $ex.Exception.InnerException.ErrorRecord.FullyQualifiedErrorId
                }

                if ($ex.Exception -is [PSDocs.Execution.InvokeDocumentException]) {
                    $errorParams['Category'] = [System.Management.Automation.ErrorCategory]::InvalidOperation;
                }

                Write-Error @errorParams;
            }
            else {
                throw $ex.Exception;
            }
        }

        Write-Verbose -Message "[Invoke-PSDocument]::END";
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

#endregion Cmdlets

#
# Internal language keywords
#

#region Keywords

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
        [Alias('When')]
        [ScriptBlock]$If,

        # Optionally create a section block even when it is empty
        [Parameter(Mandatory = $False)]
        [Switch]$Force = $False
    )

    begin {
        Write-Verbose -Message "[Doc][Section] BEGIN::";
    }

    process {

        if ($Null -eq (Get-Variable -Name Section -ErrorAction Ignore)) {
            Write-Error -Message "Section, must be defined within a document definition." -Category InvalidOperation;

            return;
        }

        $shouldProcess = $True;

        # Evaluate if the Section condition is met
        if ($Null -ne $If) {

            Write-Verbose -Message "[Doc][Section] -- If: $If";

            $conditionResult = $If.InvokeReturnAsIs();

            Write-Verbose -Message "[Doc][Section] -- If: $conditionResult";

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

            # Invoke the Section body and collect the results
            $innerResult = $Body.Invoke() | ConvertToNode;

            foreach ($r in $innerResult) {
                if ($Null -ne $r) {
                    $result.Node.Add($r);
                }
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
        Write-Verbose -Message "[Doc][Section] END:: [$($result.Node.Count)]";
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
            $result.Content = $result.Content -replace "\r\n {1,$($Matches.indent.length)}", "`r`n";
        }
        # Check for \n line endings
        elseif ($result.Content -match '^\n(?<indent> {1,})') {
            $result.Content = $result.Content -replace "\n {1,$($Matches.indent.length)}", "`n";
        }

        # Remove leading and trailing line break
        if ($result.Content -match "^(\r|\n|\r\n){1,}|(\r|\n|\r\n){1,}$") {
            $result.Content = $result.Content -replace "^(\r|\n|\r\n){1,}|(\r|\n|\r\n){1,}$", '';
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

    [CmdletBinding(DefaultParameterSetName = 'ScriptBlock')]
    param (
        [Parameter(Position = 0, Mandatory = $True, ParameterSetName = 'ScriptBlock')]
        [ScriptBlock]$Body,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True, ParameterSetName = 'Text')]
        [String]$Text
    )

    begin {
        $result = [PSDocs.Models.ModelHelper]::BlockQuote('NOTE', $Null);
    }

    process {

        if ($PSCmdlet.ParameterSetName -eq 'ScriptBlock') {
            $innerResult = $Body.InvokeWithContext($Null, $Null);

            foreach ($r in $innerResult) {
                $result.Content += $r;
            }
        }
        else {
            $result.Content += $Text;
        }
    }

    end {
        $result;
    }
}

function Warning {

    [CmdletBinding(DefaultParameterSetName = 'ScriptBlock')]
    param (
        [Parameter(Position = 0, Mandatory = $True, ParameterSetName = 'ScriptBlock')]
        [ScriptBlock]$Body,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True, ParameterSetName = 'Text')]
        [String]$Text
    )

    begin {
        $result = [PSDocs.Models.ModelHelper]::BlockQuote('WARNING', $Null);
    }

    process {

        if ($PSCmdlet.ParameterSetName -eq 'ScriptBlock') {
            $innerResult = $Body.InvokeWithContext($Null, $Null);

            foreach ($r in $innerResult) {
                $result.Content += $r;
            }
        }
        else {
            $result.Content += $Text;
        }
    }

    end {
        $result;
    }
}

function BlockQuote {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [String]$Text,

        [Parameter(Mandatory = $False)]
        [String]$Info,

        [Parameter(Mandatory = $False)]
        [String]$Title
    )

    begin {
        $result = [PSDocs.Models.ModelHelper]::BlockQuote($Info, $Title);
    }

    process {
        $result.Content += $Text;
    }

    end {
        $result;
    }
}

function Include {

    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$FileName,

        [Parameter(Mandatory = $False)]
        [String]$BaseDirectory = $PWD,

        [Parameter(Mandatory = $False)]
        [String]$Culture = $Culture,

        [Parameter(Mandatory = $False)]
        [Switch]$UseCulture = $False
    )

    process {
        $result = [PSDocs.Models.ModelHelper]::Include($BaseDirectory, $Culture, $FileName, $UseCulture);

        $result;
    }
}

function Metadata {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [AllowNull()]
        [System.Collections.IDictionary]$Body
    )
    process {
        if ($Null -eq $Body) {
            return;
        }
        # Process each key value pair in the supplied dictionary/hashtable
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
        $builder = [PSDocs.Models.ModelHelper]::Table();

        $recordIndex = 0;

        $rowData = New-Object -TypeName Collections.Generic.List[Object];

        $propertyExpression = @();

        # Prepare header if specified
        if ($Null -ne $Property -and $Property.Length -gt 0) {
            foreach ($p in $Property) {
                if ($p -is [String]) {
                    $builder.Header([String]$p);
                    $propertyExpression += $p;
                }
                elseif ($p -is [Hashtable]) {
                    $builder.Header([Hashtable]$p);
                    $propertyExpression += $builder.GetPropertyFilter($p);
                }
            }
        }
    }

    process {

        Write-Verbose -Message "[Doc][Table][$recordIndex] BEGIN::";

        Write-Verbose -Message "[Doc][Table][$recordIndex] -- Adding '$($InputObject)'";

        if ($Null -ne $InputObject) {
            $selectedObject = Select-Object -InputObject $InputObject -Property $propertyExpression;

            $rowData.Add($selectedObject);
        }

        Write-Verbose -Message "[Doc][Table][$recordIndex] END::";

        $recordIndex++;
    }

    end {
        # Extract out the header column names based on the resulting objects
        if ($Null -eq $Property) {
            $rowData | ForEach-Object -Process {
                $_.PSObject.Properties
            } | Where-Object -FilterScript {
                $_.IsGettable -and $_.IsInstance
            } | Select-Object -Unique -ExpandProperty Name | ForEach-Object -Process {
                $builder.Header([String]$_);
            }
        }

        $table = $builder.Build();

        foreach ($r in $rowData) {

            [String[]]$row = New-Object -TypeName 'String[]' -ArgumentList $table.Headers.Count;

            for ($i = 0; $i -lt $row.Length; $i++) {
                $field = GetObjectField -InputObject $r -Field $table.Headers[$i].Label -Verbose:$VerbosePreference;

                if ($Null -ne $field -and $Null -ne $field.Value) {
                    $row[$i] = $field.Value.ToString();
                }
            }

            $Null = $table.Rows.Add($row);
        }

        if ($table.Rows.Count -gt 0) {
            $table;
        }

        Write-Verbose -Message "[Doc][Table] END:: [$($table.Rows.Count)]";
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

#endregion Keywords

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
        [PSDocs.Configuration.MarkdownEncoding]$Encoding = [PSDocs.Configuration.MarkdownEncoding]::Default,

        [Parameter(Mandatory = $False)]
        [String[]]$Culture
    )

    process {
        Write-Verbose -Message "[$($MyInvocation.InvocationName)]::BEGIN";

        $fnParams = $PSBoundParameters;

        try {
            GenerateDocumentInline -Name $MyInvocation.InvocationName @fnParams;
        }
        catch {
            # Handle exceptions to provide meaningful errors

            $ex = $_;

            if ($Null -ne $ex.Exception.InnerException) {

                $errorParams = @{
                    Exception = $ex.Exception
                    Category = [System.Management.Automation.ErrorCategory]::OperationStopped
                }

                if ($ex.Exception.InnerException -is [System.Management.Automation.IContainsErrorRecord]) {
                    $errorParams['ErrorId'] = $ex.Exception.InnerException.ErrorRecord.FullyQualifiedErrorId
                }

                if ($ex.Exception -is [PSDocs.Execution.InvokeDocumentException]) {
                    $errorParams['Category'] = [System.Management.Automation.ErrorCategory]::InvalidOperation;
                }

                Write-Error @errorParams;
            }
            else {
                throw $ex.Exception;
            }
        }

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
        [Switch]$PassThru = $False,

        [Parameter(Mandatory = $False)]
        [AllowNull()]
        [PSDocs.Configuration.PSDocumentOption]$Option,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding = [PSDocs.Configuration.MarkdownEncoding]::Default,

        [Parameter(Mandatory = $False)]
        [String[]]$Culture
    )

    begin {

        # Check if the path is a directory
        if (!(Test-Path -Path $Path)) {
            Write-Error -Message $LocalizedData.PathNotFound -ErrorAction Stop;

            return;
        }

        # Get matching document scripts
        [String[]]$docScripts = GetDocumentFile -Path $Path -Verbose:$VerbosePreference;

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
            $Tag,
            $OutputPath
        );

        $PSDocs.WriteDocumentHook = {
            param ([PSDocs.Configuration.PSDocumentOption]$option, [PSDocs.Models.Document]$document)

            # Visit the document with the specified processor
            (NewMarkdownProcessor).Process($option, $document) | WriteDocumentContent -Path $document.Path -PassThru:$PassThru -Encoding:$option.Markdown.Encoding;
        }

        $PSDocs.InstanceName = $InstanceName;
        $PSDocs.Culture = $Culture;
    }

    process {

        if ($Null -eq $InputObject) {
            Write-Verbose -Message "[Doc] -- InputObject is null";
        }
        else {
            Write-Verbose -Message "[Doc] -- InputObject is of type [$($InputObject.GetType().FullName)]";
        }

        foreach ($p in $docScripts) {
            Write-Verbose -Message "[Doc] -- Processing: $p";
            InvokeTemplate -Path $p -Option $Option -InputObject $InputObject -Verbose:$VerbosePreference;
        }
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
        [Switch]$PassThru = $False,

        [Parameter(Mandatory = $False)]
        [AllowNull()]
        [PSDocs.Configuration.PSDocumentOption]$Option,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding = [PSDocs.Configuration.MarkdownEncoding]::Default,

        [Parameter(Mandatory = $False)]
        [String[]]$Culture
    )

    begin {

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
            $Null,
            $Null,
            $OutputPath
        );

        $PSDocs.WriteDocumentHook = {
            param ([PSDocs.Configuration.PSDocumentOption]$option, [PSDocs.Models.Document]$document)

            # Visit the document with the specified processor
            (NewMarkdownProcessor).Process($option, $document) | WriteDocumentContent -Path $document.Path -PassThru:$PassThru -Encoding:$option.Markdown.Encoding;
        }

        $PSDocs.InstanceName = $InstanceName;
        $PSDocs.Culture = $Culture;
    }

    process {

        if ($Null -eq $InputObject) {
            Write-Verbose -Message "[Doc] -- InputObject is null";
        }
        else {
            Write-Verbose -Message "[Doc] -- InputObject is of type [$($InputObject.GetType().FullName)]";
        }

        InvokeTemplate -Name $Name -ScriptBlock $Script:DocumentBody[$Name] -Option $Option -InputObject $InputObject -Verbose:$VerbosePreference;
    }
}

function GetDocumentFile {

    [CmdletBinding()]
    [OutputType([String])]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {
        Write-Verbose -Message "[Doc] -- Getting document definitions from: $Path";

        if (Test-Path -Path $Path -PathType Leaf) {
            Resolve-Path -Path $Path;
        }
        else {
            foreach ($p in (Get-ChildItem -Path $Path -Include '*.doc.ps1' -Recurse -File)) {
                $p.FullName;
            }
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
        $Null = $stringBuilder.Append($InputObject);
    }

    end {
        if ($PassThru) {
            Write-Verbose -Message "[Doc] -- Writing to pipeline";
            $PSCmdlet.WriteObject($stringBuilder.ToString());
        } else {
            Write-Verbose -Message "[Doc] -- Writing to path: $Path";
            [System.IO.File]::WriteAllText($Path, $stringBuilder.ToString(), $contentEncoding);
        }
    }
}

function ConvertToNode {

    [CmdletBinding()]
    [OutputType([PSDocs.Models.DocumentNode])]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [AllowNull()]
        $InputObject
    )

    process {

        if ($Null -ne $InputObject) {
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
        [Parameter(Mandatory = $True, ParameterSetName = 'Path')]
        [String]$Path,

        [Parameter(Mandatory = $True, ParameterSetName = 'ScriptBlock')]
        [String]$Name,

        [Parameter(Mandatory = $True, ParameterSetName = 'ScriptBlock')]
        [ScriptBlock]$ScriptBlock,

        [Parameter(Mandatory = $False)]
        [AllowNull()]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.PSDocumentOption]$Option
    )

    process {

        $template = $Null;

        if ($PSCmdlet.ParameterSetName -eq 'Path') {
            if (!(Test-Path -Path $Path)) {
                Write-Error -Message "Failed to find template" -Category ObjectNotFound;

                return;
            }

            $template = $Path;
        }
        else {
            $template = [String]::Concat("document ", $Name, " { ", $ScriptBlock.ToString(), "}");
        }

        try {
            # Create a PS environment for execution
            # A constrained environment will be used if DeviceGuard is enabled
            $runspace = GetRunspace -Option $Option;
            $runspace.SessionStateProxy.PSVariable.Set('InputObject', $InputObject);
            $ps = [PowerShell]::Create();
            $ps.Runspace = $runspace;

            $Null = $ps.AddScript("Set-Location -Path '$PWD';");
            $ps.Invoke();

            $Null = $ps.AddScript($template);

            try {
                $ps.Invoke();
            }
            catch {
                $baseException = $_.Exception.GetBaseException();
                $positionMessage = $Null;

                if ($baseException -is [System.Management.Automation.IContainsErrorRecord] -and $Null -ne $baseException.ErrorRecord.InvocationInfo) {
                    if (![String]::IsNullOrEmpty($baseException.ErrorRecord.InvocationInfo.PositionMessage)) {
                        $positionMessage = $baseException.ErrorRecord.InvocationInfo.PositionMessage
                    }
                }

                throw (New-Object -TypeName PSDocs.Execution.InvokeDocumentException -ArgumentList @(
                    $baseException.Message
                    $baseException
                    $Path
                    $positionMessage
                ));
            }

            # Replay verbose messages
            $ps.Streams.Verbose | ForEach-Object -Process {
                Write-Verbose -Message $_.Message;
            }

            # Replay warning messages
            $ps.Streams.Warning | ForEach-Object -Proces {
                Write-Warning -Message $_.Message;
            }
        }
        finally {
            if ($Null -ne $ps) { $ps.Dispose() }
            if ($Null -ne $runspace) { $runspace.Dispose() }
        }
    }
}

function GetRunspace {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.PSDocumentOption]$Option
    )

    process {

        $isDeviceGuard = IsDeviceGuardEnabled;

        $iss = [InitialSessionState]::CreateDefault2();
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Document',
            ${function:Document}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'ConvertToNode',
            ${function:ConvertToNode}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Section',
            ${function:Section}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Title',
            ${function:Title}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Table',
            ${function:Table}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Format-Table',
            ${function:Table}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Code',
            ${function:Code}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'BlockQuote',
            ${function:BlockQuote}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Note',
            ${function:Note}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Warning',
            ${function:Warning}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Metadata',
            ${function:Metadata}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Yaml',
            ${function:Metadata}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'Include',
            ${function:Include}
        )));
        $iss.Commands.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateFunctionEntry -ArgumentList @(
            'GetObjectField',
            ${function:GetObjectField}
        )));
        $iss.Variables.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateVariableEntry -ArgumentList @(
            'PSDocs',
            $PSDocs,
            $Null,
            [System.Management.Automation.ScopedItemOptions]::Constant
        )));
        $iss.Variables.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateVariableEntry -ArgumentList @(
            'VerbosePreference',
            [System.Management.Automation.ActionPreference]::Continue,
            $Null
        )));
        $iss.Variables.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateVariableEntry -ArgumentList @(
            'ErrorActionPreference',
            [System.Management.Automation.ActionPreference]::Stop,
            $Null
        )));
        $iss.Variables.Add((New-Object -TypeName System.Management.Automation.Runspaces.SessionStateVariableEntry -ArgumentList @(
            'PWD',
            $PWD,
            $Null
        )));
        $rs = [RunspaceFactory]::CreateRunspace($iss);
        $rs.Open();

        # If DeviceGuard is enabled, get a contrained execution environment
        if ($isDeviceGuard -or $Option.Execution.LanguageMode -eq [PSDocs.Configuration.LanguageMode]::ConstrainedLanguage) {
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

function IsDeviceGuardEnabled {

    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param (

    )

    process {

        if ((Get-Variable -Name IsMacOS -ErrorAction Ignore) -or (Get-Variable -Name IsLinux -ErrorAction Ignore)) {
            return $False;
        }

        # PowerShell 6.0.x does not support Device Guard
        if ($PSVersionTable.PSVersion -ge '6.0' -and $PSVersionTable.PSVersion -lt '6.1') {
            return $False;
        }

        return [System.Management.Automation.Security.SystemPolicy]::GetSystemLockdownPolicy() -eq [System.Management.Automation.Security.SystemEnforcementMode]::Enforce;
    }
}

function InitEditorServices {

    [CmdletBinding()]
    param (

    )

    process {

        Export-ModuleMember -Function @(
            'Section'
            'Table'
            'Metadata'
            'Title'
            'Code'
            'BlockQuote'
            'Note'
            'Warning'
            'Include'
        );
    }
}

#
# Editor services
#

if ($Null -ne (Get-Variable -Name psEditor -ErrorAction Ignore)) {
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
);

# EOM