#
# Document defintions for keyword unit tests
#

#region BlockQuote

document 'BlockQuoteSingleMarkdown' {
    'This is a single line' | BlockQuote
}

document 'BlockQuoteMultiMarkdown' {
    @('This is the first line.'
    'This is the second line.') | BlockQuote
}

document 'BlockQuoteTitleMarkdown' {
    'This is a single block quote' | BlockQuote -Title 'Test'
}

document 'BlockQuoteInfoMarkdown' {
    'This is a single block quote' | BlockQuote -Info 'Tip'
}

#endregion BlockQuote

#region Code

document 'CodeMarkdown' {
    Code {
        # This is a comment
        This is code

        # Another comment
        And code
    }
}

document 'CodeMarkdownNamedFormat' {
    Code powershell {
        Get-Content
    }
}

document 'CodeMarkdownEval' {
    $a = 1; $a += 1; $a | Code powershell;
}

#endregion Code

#region Include

document 'IncludeRelative' {
    Include tests/PSDocs.Tests/IncludeFile.md -BaseDirectory $InputObject
    Include IncludeFile2.md -BaseDirectory (Join-Path -Path $InputObject -ChildPath tests/PSDocs.Tests/)
}

document 'IncludeAbsolute' {
    Include (Join-Path -Path $InputObject -ChildPath tests/PSDocs.Tests/IncludeFile.md)
}

document 'IncludeCulture' {
    Include IncludeFile3.md -UseCulture -BaseDirectory tests/PSDocs.Tests/
}

#endregion Include

#region Metadata

document 'MetadataSingleEntry' {
    Metadata ([ordered]@{
        title = 'Test'
    })
}

document 'MetadataMultipleEntry' {
    Metadata ([ordered]@{
        value1 = 'ABC'
        value2 = 'EFG'
    })
}

document 'MetadataMultipleBlock' {
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

document 'NoMetdata' {
    Section 'Test' {
        'A test section.'
    }
}

document 'NullMetdata' {
    Metadata $Null
    Section 'Test' {
        'A test section.'
    }
}

#endregion Metadata

#region Note

document 'NoteSingleMarkdown' {
    'This is a single line' | Note
}

document 'NoteMultiMarkdown' {
    @('This is the first line.'
    'This is the second line.') | Note
}

#endregion Note

#region Warning

document 'WarningSingleMarkdown' {
    'This is a single line' | Warning
}

document 'WarningMultiMarkdown' {
    @('This is the first line.'
    'This is the second line.') | Warning
}

#endregion Warning

#region Section

document 'SectionBlockTests' {
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

document 'SectionIf' {
    Section 'Section 1' -If { $False } {
        'Content 1'
    }
    Section 'Section 2' -If { $True } {
        'Content 2'
    }
}

#endregion Section

#region Table

document 'TableTests' {
    Get-ChildItem -Path $InputObject -File | Where-Object -FilterScript { 'README.md','LICENSE' -contains $_.Name } | Format-Table -Property 'Name','PSIsContainer'
    'EOF'
}

document 'TableWithExpression' {
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

document 'TableSingleEntryMarkdown' {
    New-Object -TypeName PSObject -Property @{ Name = 'Single' } | Table -Property Name;
}

document 'TableWithNull' {
    Section 'Windows features' -Force {
        $InputObject.ResourceType.WindowsFeature | Table -Property Name,Ensure;
    }
}

document 'TableWithMultilineColumn' {
    $InputObject | Table;
}

document 'TableWithEmptyColumn' {
    'Table1'
    $InputObject | Table -Property Name,NotValue,Value
    'Table2'
    $InputObject | Table -Property Name,NotValue
    'EOF'
}

#endregion Table

#region Title

document 'SingleTitle' {
    Title 'Test title'
}

document 'MultipleTitle' {
    Title 'Title 1'
    Title 'Title 2'
}

#endregion Title
