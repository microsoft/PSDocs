#
# PSDocs Markdown processor
#

function Visit {

    [CmdletBinding()]
    param (
        [Parameter()]
        $InputObject,

        [Parameter()]
        [PSDocs.Configuration.PSDocumentOption]$Option
    )

    if ($Null -eq $InputObject) {
        return;
    }

    if ($InputObject -is [String]) {
        return VisitString($InputObject);
    }

    switch ($InputObject.Type) {
        'Document' { return VisitDocument($InputObject); }
        'Code' { return VisitCode($InputObject); }
        'Section' { return VisitSection($InputObject); }
        'Title' { return VisitTitle($InputObject); }
        'List' { return VisitList($InputObject); }
        'Table' { return VisitTable($InputObject); }
        'Note' { return VisitNote($InputObject); }
        'Warning' { return VisitWarning($InputObject); }

        default { return VisitString($InputObject); }
    }
}

function VisitString {

    [CmdletBinding()]
    [OutputType([String])]
    param (
        [Parameter(ValueFromPipeline = $True)]
        $InputObject
    )

    process {
        Write-Verbose -Message "Visit string $InputObject";

        [String]$result = $InputObject.ToString() -replace '\\', '\\';

        [String]$wrapSeparator = $Option.Markdown.WrapSeparator;

        if ($result.Contains("`n") -or $result.Contains("`r")) {
            $result = ($result -replace "\r\n", $wrapSeparator) -replace "\n|\r", $wrapSeparator;
        }

        return $result;
    }
}

function VisitSection {

    [CmdletBinding()]
    param (
        [Parameter()]
        [PSDocs.Models.Section]$InputObject
    )

    $section = $InputObject;

    Write-Verbose -Message "[Doc][Processor][Section] BEGIN::";

    Write-Verbose -Message "[Doc][Processor][Section] -- Writing section: $($section.Content)";

    # Generate markdown for the section name
    VisitString("`n$(''.PadLeft($section.Level, '#')) $($section.Content)");

    foreach ($n in $section.Node) {

        # Visit each node within the section
        Visit($n);
    }

    Write-Verbose -Message "[Doc][Processor][Section] END:: [$($section.Node.Length)]";
}

function VisitCode { 

    [CmdletBinding()]
    param (
        [Parameter()]
        [PSDocs.Models.Code]$InputObject
    )

    Write-Verbose -Message "[Doc][Processor] -- Visit code";

    if ([String]::IsNullOrEmpty($InputObject.Info)) {
        VisitString('```');
        VisitString($InputObject.Content);
        VisitString('```');
    }
    else {
        VisitString("``````$($InputObject.Info)");
        VisitString($InputObject.Content);
        VisitString('```');
    }
}

function VisitTitle {
    param ($InputObject)

    Write-Verbose -Message "[Doc][Processor] -- Visit title";

    VisitString("# $($InputObject.Title)");
}

function VisitList {
    param ($InputObject)

    Write-Verbose -Message "[Doc][Processor] -- Visit list";
    ""

    foreach ($n in $InputObject.Node) {
        [String]::Concat("- ", $This.Visit($n));
    }
}

function VisitNote {

    [CmdletBinding()]
    param (
        [Parameter()]
        [PSDocs.Models.Note]$InputObject
    )

    Write-Verbose -Message "[Doc][Processor] -- Visit note";
    
    VisitString('');
    VisitString('> [!NOTE]');

    foreach ($n in $InputObject.Content) {
        VisitString("> $n");
    }
}

function VisitWarning {

    [CmdletBinding()]
    param (
        [Parameter()]
        [PSDocs.Models.Warning]$InputObject
    )

    Write-Verbose -Message "[Doc][Processor] -- Visit warning";
    
    VisitString('');
    VisitString('> [!WARNING]');

    foreach ($w in $InputObject.Content) {
        VisitString("> $w");
    }
}

function VisitMetadata {
    param ($InputObject)

    Write-Verbose -Message "[Doc][Processor] -- Visit metadata";
    
    VisitString('---');

    foreach ($kv in $InputObject.Metadata.GetEnumerator()) {
        VisitString("$($kv.Key): $($kv.Value)");
    }

    VisitString('---');
}

function VisitTable {

    [CmdletBinding()]
    param (
        [Parameter()]
        [PSDocs.Models.Table]$InputObject
    )

    $table = $InputObject;

    Write-Verbose -Message "[Doc][Processor][Table] BEGIN::";

    $headerCount = $table.Header.Length;

    if ($Null -ne $table.Header -and $table.Header.Length -gt 0) {
        VisitString('');

        # Create header
        VisitString([String]::Concat('|', [String]::Join('|', $table.Header), '|'));
        VisitString([String]::Concat(''.PadLeft($headerCount, 'X').Replace('X', '| --- '), '|'));

        # Write each row
        foreach ($row in $table.Rows) {
            Write-Debug -Message "Generating row";

            [String[]]$columns = $row | VisitString;

            VisitString([String]::Concat('|', [String]::Join('|', $columns), '|'));
        }
    }

    Write-Verbose -Message "[Doc][Processor][Table] END:: [$($table.Rows.Count)]";
}

function VisitDocument {

    [CmdletBinding()]
    param (
        [Parameter()]
        [PSDocs.Models.Document]$InputObject
    )

    if ($Null -ne $InputObject.Metadata -and $InputObject.Metadata.Count -gt 0) {
        VisitMetadata -InputObject $InputObject;
    }

    if (![String]::IsNullOrEmpty($InputObject.Title)) {
        VisitTitle -InputObject $InputObject;
    }
}