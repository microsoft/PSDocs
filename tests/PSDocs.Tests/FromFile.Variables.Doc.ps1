
# Synopsis: Document for unit testing
document 'PSAutomaticVariables' {
    "PWD=$PWD;"
}

# Synopsis: Document for unit testing
document 'PSDocsAutomaticVariables' {
    Title '001'
    Metadata @{
        author = '002'
    }
    "Document.Title=$($document.Title);"
    "Document.Metadata=$($document.Metadata['author']);"
}
