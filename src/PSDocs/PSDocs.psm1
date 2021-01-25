#
# PSDocs module
#

Set-StrictMode -Version latest;

[PSDocs.Configuration.PSDocumentOption]::UseExecutionContext($ExecutionContext);
[PSDocs.Configuration.PSDocumentOption]::UseCurrentCulture();
$Script:UTF8_NO_BOM = New-Object -TypeName System.Text.UTF8Encoding -ArgumentList $False;

#
# Localization
#

Import-LocalizedData -BindingVariable LocalizedHelp -FileName 'PSDocs.Resources.psd1' -ErrorAction SilentlyContinue;
if ($Null -eq (Get-Variable -Name LocalizedHelp -ErrorAction SilentlyContinue)) {
    Import-LocalizedData -BindingVariable LocalizedHelp -FileName 'PSDocs.Resources.psd1' -UICulture 'en-US' -ErrorAction SilentlyContinue;
}

#
# Public functions
#

#region Cmdlets

# .ExternalHelp PSDocs-Help.xml
function Invoke-PSDocument {
    [CmdletBinding(DefaultParameterSetName = 'Input')]
    param (
        [Parameter(Mandatory = $False)]
        [Alias('m')]
        [String[]]$Module,

        # The name of the document
        [Parameter(Mandatory = $False)]
        [Alias('n')]
        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [String[]]$Tag,

        [Parameter(Mandatory = $False)]
        [String[]]$InstanceName,

        [Parameter(Mandatory = $False, ValueFromPipeline = $True, ParameterSetName = 'Input')]
        [PSObject]$InputObject,

        # The path to look for document definitions in
        [Parameter(Position = 0, Mandatory = $False)]
        [PSDefaultValue(Help = '.')]
        [Alias('p')]
        [String]$Path = $PWD,

        # The output path to save generated documentation
        [Parameter(Mandatory = $False)]
        [String]$OutputPath = $PWD,

        [Parameter(Mandatory = $False)]
        [Switch]$PassThru,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.PSDocumentOption]$Option,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding = [PSDocs.Configuration.MarkdownEncoding]::Default,

        [Parameter(Mandatory = $False)]
        [String[]]$Culture
    )
    begin {
        Write-Verbose -Message "[Invoke-PSDocument]::BEGIN";
        $pipelineReady = $False;

        # Check if the path is a directory
        if (!(Test-Path -Path $Path)) {
            Write-Error -Message $LocalizedHelp.PathNotFound -ErrorAction Stop;
            return;
        }

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] = $Option;
        }

        # Get an options object
        $Option = New-PSDocumentOption @optionParams;

        # Discover scripts in the specified paths
        $sourceParams = @{ };

        if ($PSBoundParameters.ContainsKey('Path')) {
            $sourceParams['Path'] = $Path;
        }
        if ($PSBoundParameters.ContainsKey('Module')) {
            $sourceParams['Module'] = $Module;
        }
        if ($sourceParams.Count -eq 0) {
            $sourceParams['Path'] = $Path;
        }
        $sourceParams['Option'] = $Option;
        [PSDocs.Pipeline.Source[]]$sourceFiles = GetSource @sourceParams -Verbose:$VerbosePreference;

        # Check that some matching script files were found
        if ($Null -eq $sourceFiles) {
            Write-Warning -Message $LocalizedHelp.SourceNotFound;
            return; # continue causes issues with Pester
        }

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSDocs.Configuration.LanguageMode]::ConstrainedLanguage;
        }

        # Get parameter options, which will override options from other sources
        if ($PSBoundParameters.ContainsKey('Name')) {
            $Option.Document.Include =  $Name;
        }
        if ($PSBoundParameters.ContainsKey('Tag')) {
            $Option.Document.Tag = $Tag;
        }
        if ($PSBoundParameters.ContainsKey('OutputPath') -and !$PassThru) {
            $Option.Output.Path = $OutputPath;
        }
        if ($PSBoundParameters.ContainsKey('Culture')) {
            $Option.Output.Culture = $Culture;
        }
        if ($PSBoundParameters.ContainsKey('Encoding')) {
            $Option.Markdown.Encoding = $Encoding;
        }

        $builder = [PSDocs.Pipeline.PipelineBuilder]::Invoke($sourceFiles, $Option, $PSCmdlet, $ExecutionContext);
        $builder.InstanceName($InstanceName);
        try {
            $pipeline = $builder.Build();
            if ($Null -ne $pipeline) {
                $pipeline.Begin();
                $pipelineReady = $True;
            }
        }
        catch {
            throw $_.Exception.GetBaseException();
        }
    }
    process {
        if ($pipelineReady) {
            try {
                # Process pipeline objects
                $pipeline.Process($InputObject);
            }
            catch {
                $pipeline.Dispose();
                throw;
            }
        }
    }
    end {
        if ($pipelineReady) {
            try {
                $pipeline.End();
            }
            finally {
                $pipeline.Dispose();
            }
        }
        Write-Verbose -Message "[Invoke-PSDocument]::END";
    }
}

# .ExternalHelp PSDocs-Help.xml
function Get-PSDocument {
    [CmdletBinding()]
    [OutputType([PSDocs.Definitions.IDocumentDefinition])]
    param (
        [Parameter(Mandatory = $False)]
        [Alias('m')]
        [String[]]$Module,

        [Parameter(Mandatory = $False)]
        [Switch]$ListAvailable,

        # Filter to documents with the following names
        [Parameter(Mandatory = $False)]
        [Alias('n')]
        [String[]]$Name,

        # A list of paths to check for definitions
        [Parameter(Mandatory = $False, Position = 0)]
        [Alias('p')]
        [String[]]$Path = $PWD,

        [Parameter(Mandatory = $False)]
        [PSDocs.Configuration.PSDocumentOption]$Option
    )
    begin {
        Write-Verbose -Message "[Get-PSDocument]::BEGIN";
        $pipelineReady = $False;

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };

        if ($PSBoundParameters.ContainsKey('Option')) {
            $optionParams['Option'] =  $Option;
        }

        # Get an options object
        $Option = New-PSDocumentOption @optionParams;

        # Discover scripts in the specified paths
        $sourceParams = @{ };

        if ($PSBoundParameters.ContainsKey('Path')) {
            $sourceParams['Path'] = $Path;
        }
        if ($PSBoundParameters.ContainsKey('Module')) {
            $sourceParams['Module'] = $Module;
        }
        if ($PSBoundParameters.ContainsKey('ListAvailable')) {
            $sourceParams['ListAvailable'] = $ListAvailable;
        }
        if ($sourceParams.Count -eq 0) {
            $sourceParams['Path'] = $Path;
        }
        $sourceParams['Option'] = $Option;
        [PSDocs.Pipeline.Source[]]$sourceFiles = GetSource @sourceParams -Verbose:$VerbosePreference;

        # Check that some matching script files were found
        if ($Null -eq $sourceFiles) {
            Write-Verbose -Message $LocalizedHelp.SourceNotFound;
            return; # continue causes issues with Pester
        }

        Write-Verbose -Message "[Get-PSDocument] -- Found $($sourceFiles.Length) source file(s)";

        $isDeviceGuard = IsDeviceGuardEnabled;

        # If DeviceGuard is enabled, force a contrained execution environment
        if ($isDeviceGuard) {
            $Option.Execution.LanguageMode = [PSDocs.Configuration.LanguageMode]::ConstrainedLanguage;
        }

        # Get parameter options, which will override options from other sources
        if ($PSBoundParameters.ContainsKey('Name')) {
            $Option.Document.Include =  $Name;
        }

        $builder = [PSDocs.Pipeline.PipelineBuilder]::Get($sourceFiles, $Option, $PSCmdlet, $ExecutionContext);
        try {
            $pipeline = $builder.Build();
            if ($Null -ne $pipeline) {
                $pipeline.Begin();
                $pipelineReady = $True;
            }
        }
        catch {
            throw $_.Exception.GetBaseException();
        }
    }
    end {
        if ($pipelineReady) {
            try {
                $pipeline.End();
            }
            finally {
                $pipeline.Dispose();
            }
        }
        Write-Verbose -Message "[Get-PSDocument]::END";
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
        $filteredItems = Get-ChildItem -Path (Join-Path -Path $Path -ChildPath '*') -File;
        foreach ($item in $filteredItems) {
            ReadYamlHeader -Path $item.FullName -Verbose:$VerbosePreference;
        }
    }
}

# .ExternalHelp PSDocs-Help.xml
function New-PSDocumentOption {
    [CmdletBinding(DefaultParameterSetName = 'FromPath')]
    [OutputType([PSDocs.Configuration.PSDocumentOption])]
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '', Justification = 'Creates an in memory object only')]
    param (
        [Parameter(Position = 0, Mandatory = $False, ParameterSetName = 'FromPath')]
        [String]$Path = $PWD,

        [Parameter(Mandatory = $True, ParameterSetName = 'FromOption')]
        [PSDocs.Configuration.PSDocumentOption]$Option,

        [Parameter(Mandatory = $True, ParameterSetName = 'FromDefault')]
        [Switch]$Default,

        # Options

        # Sets the Markdown.Encoding option
        [Parameter(Mandatory = $False)]
        [Alias('MarkdownEncoding')]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding = [PSDocs.Configuration.MarkdownEncoding]::Default,

        # Sets the Output.Culture option
        [Parameter(Mandatory = $False)]
        [Alias('OutputCulture')]
        [String[]]$Culture,

        # Sets the Output.Path option
        [Parameter(Mandatory = $False)]
        [String]$OutputPath
    )
    begin {
        Write-Verbose -Message "[New-PSDocumentOption] BEGIN::";

        # Get parameter options, which will override options from other sources
        $optionParams = @{ };
        $optionParams += $PSBoundParameters;

        # Remove invalid parameters
        if ($optionParams.ContainsKey('Path')) {
            $optionParams.Remove('Path');
        }
        if ($optionParams.ContainsKey('Option')) {
            $optionParams.Remove('Option');
        }
        if ($optionParams.ContainsKey('Default')) {
            $optionParams.Remove('Default');
        }
        if ($optionParams.ContainsKey('Verbose')) {
            $optionParams.Remove('Verbose');
        }
        if ($PSBoundParameters.ContainsKey('Option')) {
            $Option = [PSDocs.Configuration.PSDocumentOption]::FromFileOrEmpty($Option, $Path);
        }
        elseif ($PSBoundParameters.ContainsKey('Path')) {
            Write-Verbose -Message "Attempting to read: $Path";
            $Option = [PSDocs.Configuration.PSDocumentOption]::FromFile($Path);
        }
        elseif ($PSBoundParameters.ContainsKey('Default')) {
            $Option = [PSDocs.Configuration.PSDocumentOption]::FromDefault();
        }
        else {
            Write-Verbose -Message "Attempting to read: $Path";
            $Option = [PSDocs.Configuration.PSDocumentOption]::FromFileOrEmpty($Option, $Path);
        }
    }
    end {
        # Options
        $Option | SetOptions @optionParams -Verbose:$VerbosePreference;

        Write-Verbose -Message "[New-PSDocumentOption] END::";
    }
}

#endregion Cmdlets

#
# Internal language keywords
#

#region Keywords

# .ExternalHelp PSDocs-Help.xml
function Document {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$Name,

        [Parameter(Mandatory = $False)]
        [String[]]$Tag,

        [Parameter(Position = 1, Mandatory = $True)]
        [ScriptBlock]$Body
    )
    begin {
         # This is just a stub to improve authoring and discovery
         Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
    }
}

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
        # This is just a stub to improve authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
   }
}

function Title {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [AllowEmptyString()]
        [String]$Content
    )
    begin {
        # This is just a stub to improve authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
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
    begin {
        # This is just a stub to improve authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
   }
}

function Note {
    [CmdletBinding(DefaultParameterSetName = 'ScriptBlock')]
    [OutputType([PSDocs.Models.BlockQuote])]
    param (
        [Parameter(Position = 0, Mandatory = $True, ParameterSetName = 'ScriptBlock')]
        [ScriptBlock]$Body,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True, ParameterSetName = 'Text')]
        [String]$Text
    )
    begin {
        # This is just a stub to improve authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
   }
}

function Warning {
    [CmdletBinding(DefaultParameterSetName = 'ScriptBlock')]
    [OutputType([PSDocs.Models.BlockQuote])]
    param (
        [Parameter(Position = 0, Mandatory = $True, ParameterSetName = 'ScriptBlock')]
        [ScriptBlock]$Body,

        [Parameter(Mandatory = $True, ValueFromPipeline = $True, ParameterSetName = 'Text')]
        [String]$Text
    )
    begin {
        # This is just a stub to improve authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
   }
}

function BlockQuote {
    [CmdletBinding()]
    [OutputType([PSDocs.Models.BlockQuote])]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [String]$Text,

        [Parameter(Mandatory = $False)]
        [String]$Info,

        [Parameter(Mandatory = $False)]
        [String]$Title
    )
    begin {
        # This is just a stub to improve authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
   }
}

function Include {
    [CmdletBinding()]
    [OutputType([PSDocs.Models.Include])]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$FileName,

        [Parameter(Mandatory = $False)]
        [String]$BaseDirectory = $PWD,

        [Parameter(Mandatory = $False)]
        [String]$Culture = $Culture,

        [Parameter(Mandatory = $False)]
        [Switch]$UseCulture = $False,

        [Parameter(Mandatory = $False)]
        [System.Collections.IDictionary]$Replace
    )
    begin {
        # This is just a stub to improve authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
   }
}

function Metadata {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [AllowNull()]
        [System.Collections.IDictionary]$Body
    )
    begin {
        # This is just a stub to improve authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
   }
}

function Table {
    [CmdletBinding()]
    [OutputType([PSDocs.Models.Table])]
    param (
        [Parameter(Mandatory = $False, ValueFromPipeline = $True)]
        [AllowNull()]
        [Object]$InputObject,

        [Parameter(Mandatory = $False, Position = 0)]
        [Object[]]$Property
    )
    begin {
        # This is just a stub to improve authoring and discovery
        Write-Error -Message $LocalizedHelp.KeywordOutsideEngine -Category InvalidOperation;
   }
}

#endregion Keywords

#
# Helper functions
#

function SetOptions {
    [CmdletBinding()]
    [OutputType([PSDocs.Configuration.PSDocumentOption])]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [PSDocs.Configuration.PSDocumentOption]$InputObject,

        # Options

        # Sets the Markdown.Encoding option
        [Parameter(Mandatory = $False)]
        [ValidateSet('Default', 'UTF8', 'UTF7', 'Unicode', 'UTF32', 'ASCII')]
        [PSDocs.Configuration.MarkdownEncoding]$Encoding = 'Default',

        # Sets the Output.Culture option
        [Parameter(Mandatory = $False)]
        [String[]]$Culture,

        # Sets the Output.Path option
        [Parameter(Mandatory = $False)]
        [String]$OutputPath
    )
    process {
        # Options

        # Sets option Markdown.Encoding
        if ($PSBoundParameters.ContainsKey('Encoding')) {
            $InputObject.Markdown.Encoding = $Encoding;
        }

        # Sets option Output.Culture
        if ($PSBoundParameters.ContainsKey('Culture')) {
            $InputObject.Output.Culture = $Culture;
        }

        # Sets option Output.Path
        if ($PSBoundParameters.ContainsKey('OutputPath')) {
            $InputObject.Output.Path = $OutputPath;
        }

        return $InputObject;
    }
}

function InitDocumentContext {
    [CmdletBinding()]
    param ()
    process {

        if ($Null -eq (Get-Variable -Name DocumentBody -Scope Script -ErrorAction SilentlyContinue)) {
            $Script:DocumentBody = @{ };
        }
    }
}

# Get a list of rule script files in the matching paths
function GetSource {
    [CmdletBinding()]
    [OutputType([PSDocs.Pipeline.Source])]
    param (
        [Parameter(Mandatory = $False)]
        [String[]]$Path,

        [Parameter(Mandatory = $False)]
        [String[]]$Module,

        [Parameter(Mandatory = $False)]
        [Switch]$ListAvailable,

        [Parameter(Mandatory = $False)]
        [String]$Culture,

        [Parameter(Mandatory = $False)]
        [Switch]$PreferPath = $False,

        [Parameter(Mandatory = $False)]
        [Switch]$PreferModule = $False,

        [Parameter(Mandatory = $True)]
        [PSDocs.Configuration.PSDocumentOption]$Option
    )
    process {
        $builder = [PSDocs.Pipeline.PipelineBuilder]::Source($Option, $PSCmdlet, $ExecutionContext);
        if ($PSBoundParameters.ContainsKey('Path')) {
            try {
                $builder.Directory($Path);
            }
            catch {
                throw $_.Exception.GetBaseException();
            }
        }

        $moduleParams = @{};
        if ($PSBoundParameters.ContainsKey('Module')) {
            $moduleParams['Name'] = $Module;

            # Determine if module should be automatically loaded
            if (GetAutoloadPreference) {
                foreach ($m in $Module) {
                    if ($Null -eq (GetModule -Name $m)) {
                        LoadModule -Name $m -Verbose:$VerbosePreference;
                    }
                }
            }
        }

        if ($PSBoundParameters.ContainsKey('ListAvailable')) {
            $moduleParams['ListAvailable'] = $ListAvailable.ToBool();
        }

        if ($moduleParams.Count -gt 0 -or $PreferModule) {
            $modules = @(GetModule @moduleParams);
            $builder.Module($modules);
        }
        $builder.Build();
    }
}

function GetAutoloadPreference {
    [CmdletBinding()]
    [OutputType([System.Boolean])]
    param ()
    process {
        $v = Microsoft.PowerShell.Utility\Get-Variable -Name 'PSModuleAutoLoadingPreference' -ErrorAction SilentlyContinue;
        return ($Null -eq $v) -or ($v.Value -eq [System.Management.Automation.PSModuleAutoLoadingPreference]::All);
    }
}

function GetModule {
    [CmdletBinding()]
    [OutputType([System.Management.Automation.PSModuleInfo])]
    param (
        [Parameter(Mandatory = $False)]
        [String[]]$Name,

        [Parameter(Mandatory = $False)]
        [Switch]$ListAvailable = $False
    )
    process {
        $moduleResults = (Microsoft.PowerShell.Core\Get-Module @PSBoundParameters | Microsoft.PowerShell.Core\Where-Object -FilterScript {
            'PSDocs-documents' -in $_.Tags
        } | Microsoft.PowerShell.Utility\Group-Object -Property Name)

        if ($Null -ne $moduleResults) {
            foreach ($m in $moduleResults) {
                @($m.Group | Microsoft.PowerShell.Utility\Sort-Object -Descending -Property Version)[0];
            }
        }
    }
}

function LoadModule {
    [CmdletBinding()]
    [OutputType([void])]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Name
    )
    process{
        $Null = GetModule -Name $Name -ListAvailable | Microsoft.PowerShell.Core\Import-Module -Global;
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
        if (![String]::IsNullOrEmpty($content) -and $content -match '^(---(\r|\n|\r\n)(?<yaml>([A-Z0-9]{1,}:[A-Z0-9 ]{1,}(\r|\n|\r\n){0,}){1,})(\r|\n|\r\n)---(\r|\n|\r\n))') {
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
    param ()
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
    'Get-PSDocument'
    'Get-PSDocumentHeader'
    'New-PSDocumentOption'
);

# EOM
