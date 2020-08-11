#
# Document defintions for benchmarks
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

document 'NoteScriptBlockMarkdown' {
    Note {
        'This is a single line'
    }
}

#endregion Note

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

document 'SectionWhen' {
    Section 'Section 1' -If { $False } {
        'Content 1'
    }
    Section 'Section 2' -If { $True } {
        'Content 2'
    }
    # Support for When alias of If
    Section 'Section 3' -When { $True } {
        'Content 3'
    }
}

#endregion Section

#region Table

#endregion Table
