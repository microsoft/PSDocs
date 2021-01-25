#
# Document defintions for keyword unit tests
#

#region BlockQuote

Document 'BlockQuoteSingleMarkdown' {
    'Begin'
    'This is a single line' | BlockQuote
    'End'
}

Document 'BlockQuoteMultiMarkdown' {
    'Begin'
    @('This is the first line.'
    'This is the second line.') | BlockQuote
    'End'
}

Document 'BlockQuoteTitleMarkdown' {
    'Begin'
    'This is a single block quote' | BlockQuote -Title 'Test'
    'End'
}

Document 'BlockQuoteInfoMarkdown' {
    'Begin'
    'This is a single block quote' | BlockQuote -Info 'Tip'
    'End'
}

#endregion BlockQuote

#region Code

Document 'CodeMarkdown' {
    'Begin'
    Code {
        # This is a comment
        This is code

        # Another comment
        And code
    }
    'End'
}

Document 'CodeMarkdownNamedFormat' {
    'Begin'
    Code powershell {
        Get-Content
    }
    'End'
}

Document 'CodeMarkdownEval' {
    'Begin'
    $a = 1; $a += 1; $a | Code powershell;
    'End'
}

Document 'CodeInclude' {
    'Begin'
    Include 'psdocs.yml' -BaseDirectory $PSScriptRoot | Code 'yaml'
    'End'
}

Document 'CodeJson' {
    $a = [PSCustomObject]@{
        Name = 'Value'
    }
    $a | Code 'json'
}

Document 'CodeYaml' {
    $a = [PSCustomObject]@{
        Name = 'Value'
    }
    $a | Code 'yaml'
}

#endregion Code

#region Include

Document 'IncludeRelative' {
    Include tests/PSDocs.Tests/IncludeFile.md -BaseDirectory $TargetObject
    Include IncludeFile2.md -BaseDirectory (Join-Path -Path $TargetObject -ChildPath tests/PSDocs.Tests/)
}

Document 'IncludeAbsolute' {
    Include (Join-Path -Path $TargetObject -ChildPath tests/PSDocs.Tests/IncludeFile.md)
}

Document 'IncludeCulture' {
    Include IncludeFile3.md -UseCulture -BaseDirectory tests/PSDocs.Tests/
}

# Synopsis: Test Include keyword with -ErrorAction SilentlyContinue
Document 'IncludeOptional' {
    Include 'NotFile.md' -ErrorAction SilentlyContinue;
}

# Synopsis: Test Include keyword with missing file
Document 'IncludeRequired' {
    Include 'NotFile.md';
}

# Synopsis: Include and replace tokens
Document 'IncludeReplace' {
    Include IncludeFile2.md -BaseDirectory $PSScriptRoot -Replace @{
        'second' = 'third'
    }
}

#endregion Include

#region Metadata

Document 'MetadataSingleEntry' {
    Metadata ([ordered]@{
        title = 'Test'
    })
}

Document 'MetadataMultipleEntry' {
    Metadata ([ordered]@{
        value1 = 'ABC'
        value2 = 'EFG'
    })
}

Document 'MetadataMultipleBlock' {
    Metadata ([ordered]@{
        value1 = 'ABC'
    })
    Section 'Test' {
        'A test section spliting metadata blocks.'
    }
    Metadata @{
        value2 = 'EFG'
    }
}

Document 'NoMetdata' {
    Section 'Test' {
        'A test section.'
    }
}

Document 'NullMetdata' {
    Metadata $Null
    Section 'Test' {
        'A test section.'
    }
}

#endregion Metadata

#region Note

Document 'NoteSingleMarkdown' {
    'This is a single line' | Note
}

Document 'NoteMultiMarkdown' {
    @('This is the first line.'
    'This is the second line.') | Note
}

#endregion Note

#region Warning

Document 'WarningSingleMarkdown' {
    'This is a single line' | Warning
}

Document 'WarningMultiMarkdown' {
    @('This is the first line.'
    'This is the second line.') | Warning
}

#endregion Warning

#region Section

Document 'SectionBlockTests' {
    Section 'SingleLine' {
        'This is a single line markdown section.'
    }
    Section 'MultiLine' {
        "This is a multiline`r`ntest."
    }
    Section 'Empty' {
    }
    Section 'Forced' -Force {
    }
}

Document 'SectionIf' {
    Section 'Section 1' -If { $False } {
        'Content 1'
    }
    Section 'Section 2' -If { $True } {
        'Content 2'
    }
}

#endregion Section

#region Table

Document 'TableTests' {
    Get-ChildItem -Path $TargetObject -File | Where-Object -FilterScript { 'README.md','LICENSE' -contains $_.Name } | Format-Table -Property 'Name','PSIsContainer'
    'EOF'
}

Document 'TableWithExpression' {
    $object = [PSCustomObject]@{
        Name = 'Dummy'
        Property = @{
            Value1 = 1
            Value2 = 2
        }
        Value3 = 3
    }
    $object | Table -Property Name,@{ Label = 'Value1'; Alignment = 'Left'; Width = 10; Expression = { $_.Property.Value1 }},@{ Name = 'Value2'; Alignment = 'Center'; Expression = { $_.Property.Value2 }},@{ Label = 'Value3'; Expression = { $_.Value3 }; Alignment = 'Right'; };
    'EOF'
}

Document 'TableSingleEntryMarkdown' {
    New-Object -TypeName PSObject -Property @{ Name = 'Single' } | Table -Property Name;
}

Document 'TableWithNull' {
    Section 'Windows features' -Force {
        $TargetObject.ResourceType.WindowsFeature | Table -Property Name,Ensure;
    }
}

Document 'TableWithMultilineColumn' {
    $TargetObject | Table;
}

Document 'TableWithEmptyColumn' {
    'Table1'
    $TargetObject | Table -Property Name,NotValue,Value
    'Table2'
    $TargetObject | Table -Property Name,NotValue
    'EOF'
}

#endregion Table

#region Title

Document 'SingleTitle' {
    Title 'Test title'
}

Document 'MultipleTitle' {
    Title 'Title 1'
    Title 'Title 2'
}

# Synopsis: Tests Title with empty or null string
Document 'EmptyTitle' {
    $value = ''
    Title $notValue
    Title $value
}

#endregion Title
