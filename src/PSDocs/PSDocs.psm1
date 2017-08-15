#
# PSDocs module
#

#
# Localization
#

$LocalizedData = data {

}

Import-LocalizedData -BindingVariable LocalizedData -FileName 'PSDocs.Resources.psd1' -ErrorAction SilentlyContinue;

#
# Public functions
#

# Implement the document keyword
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

function Invoke-PSDocument {

    [CmdletBinding()]
    param (
        # The name of the document
        [Parameter(Mandatory = $True)]
        [String]$Name,

        [Parameter(Mandatory = $False)]
        [String]$InstanceName,
        
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $False)]
        [Object]$ConfigurationData,

        # The path to look for document definitions in
        [Parameter(Mandatory = $False)]
        [String]$Path = $PWD,

        # The output path to save generated documentation
        [Parameter(Mandatory = $False)]
        [String]$OutputPath,

        [Parameter(Mandatory = $False)]
        [ValidateNotNull()]
        [System.Collections.Generic.Dictionary[String, ScriptBlock]]$Function
    )

    process {
        Write-Verbose -Message "[Invoke-PSDocument]::BEGIN";

        Write-Verbose -Message "[Invoke-PSDocument] -- Processing: $InstanceName";

        $fnParams = $PSBoundParameters;

        GenerateDocument @fnParams;
        
        Write-Verbose -Message "[Invoke-PSDocument]::END";
    }
}

#
# Internal language keywords
#

# Implement the section keyword
function Section {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [String]$Name,

        [Parameter(Position = 1, Mandatory = $True)]
        [ScriptBlock]$Body,

        [Parameter(Mandatory = $False)]
        [ScriptBlock]$When
    )

    begin {
        Write-Verbose -Message "[Doc][Section] BEGIN::";
    }

    process {

        $shouldProcess = $True;

        if ($Null -ne $When) {

            Write-Verbose -Message "[Doc][Section] -- When: $When";

            $conditionResult = $When.InvokeReturnAsIs();

            Write-Verbose -Message "[Doc][Section] -- When: $conditionResult";

            if (($Null -eq $conditionResult) -or ($conditionResult -is [System.Boolean] -and $conditionResult -eq $False)) {
                $shouldProcess = $False;
            }
        }

        if ($shouldProcess) {
            Write-Verbose -Message "[Doc][Section] -- Adding section: $Name";

            $result = New-Object -TypeName PSObject -Property @{ Content = $Name; Type = 'Section'; Node = @(); Level = ($Section.Level+1) };

            $Section = $result;

            $innerResult = $Body.Invoke();

            foreach ($r in $innerResult) {
                $result.Node += $r;
            }

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
        [String]$Title
    )

    process {
        $result = New-Object -TypeName PSObject -Property @{ Type = 'Title'; Content = $Title; };

        $result;
    }
}

function Code {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [ScriptBlock]$Body
    )

    process {
        $result = New-Object -TypeName PSObject -Property @{ Type = 'Code'; Content = ''; };

        $innerResult = $Body.InvokeWithContext($Null, $Null);

        foreach ($r in $innerResult) {
            $result.Content += $r;
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

function Table {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [String[]]$Header,

        [Parameter(Position = 1, Mandatory = $True)]
        [ScriptBlock]$Body
    )

    process {


        $table = New-Object -TypeName PSObject -Property @{ Type = 'Table'; Header = $Header; Rows = (New-Object -TypeName Collections.Generic.List[String[]]); ColumnCount = 0; };

        $functionsToDefine = New-Object -TypeName 'System.Collections.Generic.Dictionary[string,ScriptBlock]'([System.StringComparer]::OrdinalIgnoreCase);
        $functionsToDefine.Add('Row', ${function:Row});

        [PSVariable[]]$variablesToDefine = @(
            New-Object -TypeName PSVariable -ArgumentList ('Table', $table)
        );

        $Body.InvokeWithContext($functionsToDefine, $variablesToDefine) | Out-Null;

        # foreach ($r in $innerResult) {
        #     $result.Rows += $r;
        # }

        $table;
    }
}

function Row {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [AllowNull()]
        [Object]$InputObject,

        [Parameter(Mandatory = $False, Position = 0)]
        [String[]]$Property
    )

    begin {
        $rowIndex = $Table.Rows.Count;

        if ($Null -eq $Table) {
            Write-Error -Message 'Failed to find table'
        }

        [String[]]$objectFields = @($Table.Header);

        if ($Null -ne $Property -and $Property.Count -gt 0) {

            for ($i = 0; $i -lt $objectFields.Count -and $i -lt $Property.Count; $i++)
            {
                $objectFields[$i] = $Property[$i];
            }
        }
    }

    process {

        Write-Verbose -Message "[Row][$rowindex]`t-- Adding  '$($InputObject)'";

        [String[]]$row = , [String]::Empty * $Table.Header.Count;

        if ($Null -eq $InputObject) {

        } else {
            
            for ($i = 0; $i -lt $row.Count; $i++) {
                $field = GetObjectField -InputObject $InputObject -Field $objectFields[$i] -Verbose:$VerbosePreference;

                if ($Null -ne $field -and $Null -ne $field.Value) {
                    $row[$i] = $field.Value;
                }
            }
        }

        $Table.Rows.Add($row);
    }

    end {
        
    }
}

function Note {

    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [ScriptBlock]$Body
    )

    process {

        $result = New-Object -TypeName PSObject -Property @{ Type = 'Note'; Node = @(); Content = ''; };

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

        $result = New-Object -TypeName PSObject -Property @{ Type = 'Warning'; Node = @(); Content = ''; };

        $innerResult = $Body.InvokeWithContext($Null, $Null);

        foreach ($r in $innerResult) {
            $result.Content += $r;
        }

        $result;
    }
}

function Yaml {

    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $True)]
        [Hashtable]$Body
    )

    process {

        $result = New-Object -TypeName PSObject -Property @{ Type = 'Yaml'; Node = @(); Content = $Body; };

        $result;
    }
}

function FormatTable {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [Object]$InputObject,

        [Parameter(Mandatory = $False, Position = 0)]
        [String[]]$Property
    )

    begin {
        Write-Verbose -Message "[Doc][FormatTable] BEGIN::";

        $table = New-Object -TypeName PSObject -Property @{ Type = 'Table'; Header = @(); Rows = (New-Object -TypeName Collections.Generic.List[String[]]); ColumnCount = 0; };

        $recordIndex = 0;

        $rowData = New-Object -TypeName Collections.Generic.List[Object];

        # if ($Property -is [Hashtable[]]) {
        #     $Property = $Property -as [Hashtable[]];
        # } else {
        #     $Property = $Property -as [String[]];
        # }
    }

    process {

        Write-Verbose -Message "[Doc][FormatTable][$recordIndex] BEGIN::";

        Write-Verbose -Message "[Doc][FormatTable][$recordIndex] -- Adding  '$($InputObject)'";

        if ($Null -ne $InputObject) {
            $selectedObject = Select-Object -InputObject $InputObject -Property $Property;

            $rowData.Add($selectedObject);
        }

        Write-Verbose -Message "[Doc][FormatTable][$recordIndex] END::";

        $recordIndex++;
    }

    end {
        $headers = $rowData | ForEach-Object -Process {
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

        Write-Verbose -Message "[Doc][FormatTable] END::";
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

        [Parameter(Mandatory = $False)]
        [Object]$ConfigurationData,

        [Parameter(Mandatory = $False)]
        [String]$OutputPath
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
        [String]$InstanceName,

        [Parameter(Mandatory = $True)]
        [PSObject]$InputObject,

        [Parameter(Mandatory = $False)]
        [Object]$ConfigurationData,

        [Parameter(Mandatory = $False)]
        [String]$Path = $PWD,

        [Parameter(Mandatory = $False)]
        [String]$OutputPath,

        [Parameter(Mandatory = $False)]
        [System.Collections.Generic.Dictionary[String, ScriptBlock]]$Function
    )

    begin {
        if (!$Script:DocumentBody.ContainsKey($Name)) {
            Write-Error -Message "Failed to get document body.";

            return;
        }

        [Hashtable]$parameter = $Null;

        if ($ConfigurationData -is [Hashtable]) {
            $parameter = $ConfigurationData
        } elseif ($ConfigurationData -is [String] -and (Test-Path -Path $ConfigurationData -File)) {
            $parentPath = Split-Path -Parent -Path $ConfigurationData;
            $leafPath = Split-Path -Left -Path $ConfigurationData;

            Import-LocalizedData -BindingVariable parameter -BaseDirectory $parentPath -FileName $leafPath;
        }

        # Process pipeline input

        # $pi = ParseMofDocument -Path $FileInfo.FullName -Verbose:$VerbosePreference;

        # Run document

        $body = $Script:DocumentBody[$Name];

        $functionsToDefine = New-Object -TypeName 'System.Collections.Generic.Dictionary[String,ScriptBlock]'([System.StringComparer]::OrdinalIgnoreCase);

        if ($Null -ne $Function -and $Function.Count -gt 0) {
            foreach ($fn in $Function) {
                $functionsToDefine.Add($fn.Key, $fn.Value);
            }
        }

        $functionsToDefine['Section'] = ${function:Section};
        $functionsToDefine['Title'] = ${function:Title};
        $functionsToDefine['List'] = ${function:List};
        $functionsToDefine['Code'] = ${function:Code};
        $functionsToDefine['Table'] = ${function:Table};
        $functionsToDefine['Note'] = ${function:Note};
        $functionsToDefine['Warning'] = ${function:Warning};
        $functionsToDefine['Yaml'] = ${function:Yaml};
        $functionsToDefine['Format-Table'] = ${function:FormatTable};
        $functionsToDefine['Format-List'] = ${function:FormatList};
    }

    process {

        $instances = $InstanceName;

        $Section = @{ Level = 1; };

        foreach ($instance in $instances) {
            [PSVariable[]]$variablesToDefine = @(
                New-Object -TypeName PSVariable -ArgumentList ('NodeName', $InstanceName)
                New-Object -TypeName PSVariable -ArgumentList ('InputObject', $InputObject)
                New-Object -TypeName PSVariable -ArgumentList ('Parameter', $parameter)
                New-Object -TypeName PSVariable -ArgumentList ('Section', $Section)
            );

            $innerResult = $body.InvokeWithContext($functionsToDefine, $variablesToDefine);

            $dom = New-Object -TypeName PSObject -Property @{ Node = $innerResult };

            ParseDom -Dom $dom -Processor (NewMarkdownProcessor) -Verbose:$VerbosePreference | Set-Content -Path "$OutputPath\$InstanceName.md"
        }
    }
}

function ParseDom {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [PSObject]$Dom,

        [Parameter(Mandatory = $True)]
        [PSObject]$Processor
    )

    process {

        $innerResult = $Dom.Node | ForEach-Object -Process {
            $node = $_;

            Write-Verbose -Message "[Doc][ParseDom] -- Processing node";

            if ($Null -ne $node) {
                $Processor.Visit($node);
            }
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
        $result = New-Object -TypeName PSObject

        Add-Member -InputObject $result -MemberType ScriptMethod -Name 'String' -Value {
            param ($Input)
            Write-Verbose -Message "Visit string $Input";

            if ($Input -isnot [String]) {
                return $Input.ToString();
            }

            return $Input;
        }
        
        Add-Member -InputObject $result -MemberType ScriptMethod -Name 'Section' -Value {
            param ($Input)
            Write-Verbose -Message "[Doc][Processor] -- Visit section";
            "`n$(''.PadLeft($Input.Level, '#')) $($Input.Content)";

            foreach ($n in $Input.Node) {

                Write-Verbose -Message "[Doc][Processor] -- Visit section node";

                $This.Visit($n);
            }
        }

        Add-Member -InputObject $result -MemberType ScriptMethod -Name 'Code' -Value {
            param ($Input)

            Write-Verbose -Message "[Doc][Processor] -- Visit code";
            "   $($Input.Content)";
        }

        Add-Member -InputObject $result -MemberType ScriptMethod -Name 'Title' -Value {
            param ($Input)

            Write-Verbose -Message "[Doc][Processor] -- Visit title";

            "# $($Input.Content)";
        }

        Add-Member -InputObject $result -MemberType ScriptMethod -Name 'List' -Value {
            param ($Input)

            Write-Verbose -Message "[Doc][Processor] -- Visit list";
            ""

            foreach ($n in $Input.Node) {
                [String]::Concat("- ", $This.Visit($n));
            }
        }

        Add-Member -InputObject $result -MemberType ScriptMethod -Name 'Table' -Value {
            param ($Input)

            Write-Verbose -Message "[Doc][Processor] -- Visit table";

            $table = $Input;

            $headerCount = $table.Header.Length;

            ""

            # Create header
            Write-Verbose -Message "Writing table header";
            [String]::Concat('|', [String]::Join('|', $table.Header), '|');
            [String]::Concat(''.PadLeft($headerCount, 'X').Replace('X', '| --- '), '|');

            Write-Verbose -Message "Writing $($table.Rows.Count) rows";

            foreach ($row in $table.Rows) {
                Write-Debug -Message "Generating row";

                [String]::Concat('|', [String]::Join('|', [String[]]$row), '|');
            }
        }

        Add-Member -InputObject $result -MemberType ScriptMethod -Name 'Note' -Value {
            param ($Input)

            Write-Verbose -Message "[Doc][Processor] -- Visit note";
            
            '';
            '> [!NOTE]';
            "> $($Input.Content)";
        }

        Add-Member -InputObject $result -MemberType ScriptMethod -Name 'Warning' -Value {
            param ($Input)

            Write-Verbose -Message "[Doc][Processor] -- Visit warning";
            
            '';
            '> [!WARNING]';
            "> $($Input.Content)";
        }

        Add-Member -InputObject $result -MemberType ScriptMethod -Name 'Yaml' -Value {
            param ($Input)

            Write-Verbose -Message "[Doc][Processor] -- Visit yaml";
            
            '---';

            foreach ($kv in $Input.Content.GetEnumerator()) {
                "$($kv.Key): $($kv.Value)";
            }

            '---';
        }

        Add-Member -InputObject $result -MemberType ScriptMethod -Name 'Visit' -Value {
            param ($Input)

            if ($Null -eq $Input) {
                return;
            }

            if ($Input -is [String]) {
                return $This.String($Input);
            }

            switch ($Input.Type) {
                'Code' { return $This.Code($Input); }
                'Section' { return $This.Section($Input); }
                'Title' { return $This.Title($Input); }
                'List' { return $This.List($Input); }
                'Table' { return $This.Table($Input); }
                'Note' { return $This.Note($Input); }
                'Warning' { return $This.Warning($Input); }
                'Yaml' { return $This.Yaml($Input); }

                default { return $This.String($Input); }
            }
        }


        # $result.Methods.Add((New-Object -TypeName PSScriptMethod -ArgumentList @('Default', { })));

        $result;
    }
}

#
# Export module
#

Export-ModuleMember -Function 'Document','Invoke-PSDocument';

# EOM