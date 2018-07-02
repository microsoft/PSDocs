#
# PSDocs Markdown processor
#

function Visit {

    [CmdletBinding()]
    param (
        [Parameter()]
        [Object]$InputObject,

        [Parameter()]
        [PSDocs.Configuration.PSDocumentOption]$Option
    )

    if ($Null -eq $InputObject) {
        return;
    }

    if ($InputObject -is [String]) {
        return VisitString -InputObject $InputObject;
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

        default { return VisitString -InputObject $InputObject; }
    }
}

function VisitString {

    [CmdletBinding()]
    [OutputType([String])]
    param (
        [Parameter(ValueFromPipeline = $True)]
        $InputObject,

        [Parameter(Mandatory = $False)]
        [Switch]$ShouldWrap = $False
    )

    process {
        Write-Verbose -Message "Visit string $InputObject";

        [String]$result = $InputObject.ToString() -replace '\\', '\\';

        if ($ShouldWrap -and ($result.Contains("`n") -or $result.Contains("`r"))) {

            # Get the wrap separator
            [String]$wrapSeparator = $Option.Markdown.WrapSeparator;

            # Replace new line characters with separator
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

    $sectionPadding = ''.PadLeft($section.Level, '#');

    # Generate markdown for the section name
    VisitString -InputObject "`r`n$sectionPadding $($section.Content)";

    foreach ($n in $section.Node) {

        # Visit each node within the section
        Visit -InputObject $n -Option $Option;
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
        VisitString -InputObject '```';
        VisitString -InputObject $InputObject.Content;
        VisitString -InputObject '```';
    }
    else {
        VisitString -InputObject "``````$($InputObject.Info)";
        VisitString -InputObject $InputObject.Content;
        VisitString -InputObject '```';
    }
}

function VisitTitle {
    param ($InputObject)

    Write-Verbose -Message "[Doc][Processor] -- Visit title";

    VisitString -InputObject "# $($InputObject.Title)";
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

    VisitString -InputObject '';
    VisitString -InputObject '> [!NOTE]';

    foreach ($n in $InputObject.Content) {
        VisitString -InputObject "> $n";
    }
}

function VisitWarning {

    [CmdletBinding()]
    param (
        [Parameter()]
        [PSDocs.Models.Warning]$InputObject
    )

    Write-Verbose -Message "[Doc][Processor] -- Visit warning";

    VisitString -InputObject '';
    VisitString -InputObject '> [!WARNING]';

    foreach ($w in $InputObject.Content) {
        VisitString -InputObject "> $w";
    }
}

function VisitMetadata {
    param ($InputObject)

    Write-Verbose -Message "[Doc][Processor] -- Visit metadata";

    VisitString -InputObject '---';

    foreach ($kv in $InputObject.Metadata.GetEnumerator()) {
        VisitString -InputObject "$($kv.Key): $($kv.Value)";
    }

    VisitString -InputObject '---';
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
        VisitString -InputObject '';

        # Create header
        VisitString -InputObject ([String]::Concat('|', [String]::Join('|', $table.Header), '|'));
        VisitString -InputObject ([String]::Concat(''.PadLeft($headerCount, 'X').Replace('X', '| --- '), '|'));

        # Write each row
        foreach ($row in $table.Rows) {
            Write-Debug -Message "Generating row";

            [String[]]$columns = $row | VisitString -ShouldWrap;

            VisitString -InputObject ([String]::Concat('|', [String]::Join('|', $columns), '|'));
        }

        VisitString -InputObject '';
    }

    Write-Verbose -Message "[Doc][Processor][Table] END:: [$($table.Rows.Count)]";
}

function VisitDocument {

    [CmdletBinding()]
    param (
        [Parameter()]
        [PSDocs.Models.Document]$InputObject
    )

    $document = $InputObject;

    if ($Null -ne $document.Metadata -and $document.Metadata.Count -gt 0) {
        VisitMetadata -InputObject $document;
    }

    if (![String]::IsNullOrEmpty($document.Title)) {
        VisitTitle -InputObject $document;
    }

    foreach ($n in $document.Node) {

        # Visit each node within the document
        Visit -InputObject $n -Option $Option;
    }
}