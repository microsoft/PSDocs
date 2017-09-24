#
# PSDocs Markdown processor
#

function Visit {

    param (
        $InputObject
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
        'Yaml' { return VisitYaml($InputObject); }

        default { return VisitString($InputObject); }
    }
}

function VisitString {

    param (
        $InputObject
    )

    Write-Verbose -Message "Visit string $InputObject";

    if ($InputObject -isnot [String]) {
        return $InputObject.ToString() -replace '\\', '\\';
    }

    return $InputObject -replace '\\', '\\';
}

function VisitSection {

    param (
        $InputObject
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

    param (
        $InputObject
    )

    Write-Verbose -Message "[Doc][Processor] -- Visit code";

    VisitString("   $($InputObject.Content)");
}

function VisitTitle {
    param ($InputObject)

    Write-Verbose -Message "[Doc][Processor] -- Visit title";

    VisitString("# $($InputObject.Content)");
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
    param ($InputObject)

    Write-Verbose -Message "[Doc][Processor] -- Visit note";
    
    VisitString('');
    VisitString('> [!NOTE]');

    foreach ($n in $InputObject.Content) {
        VisitString("> $n");
    }
}

function VisitWarning {
    param ($InputObject)

    Write-Verbose -Message "[Doc][Processor] -- Visit warning";
    
    VisitString('');
    VisitString('> [!WARNING]');

    foreach ($w in $InputObject.Content) {
        VisitString("> $w");
    }
}

function VisitYaml {
    param ($InputObject)

    Write-Verbose -Message "[Doc][Processor] -- Visit yaml";
    
    VisitString('---');

    foreach ($kv in $InputObject.Metadata.GetEnumerator()) {
        VisitString("$($kv.Key): $($kv.Value)");
    }

    VisitString('---');
}
   
function VisitTable {

    param (
        $InputObject
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

            VisitString([String]::Concat('|', [String]::Join('|', [String[]]$row), '|'));
        }
    }

    Write-Verbose -Message "[Doc][Processor][Table] END:: [$($table.Rows.Count)]";
}

function VisitDocument {

    param (
        $InputObject
    )

    VisitYaml -InputObject $InputObject;
}