
# Synopsis: Document for unit testing
Document 'PSAutomaticVariables' {
    "PWD=$PWD;"
    "PSScriptRoot=$PSScriptRoot;"
    "PSCommandPath=$PSCommandPath;"
}

# Synopsis: Test $PSDocs variable
Document 'PSDocsVariable' {
    $PSDocs.Format("TargetObject.Name={0};", $PSDocs.TargetObject.Name);
}

# Synopsis: Test $Document variable
Document 'PSDocsDocumentVariable' {
    Title '001'
    Metadata @{
        author = '002'
    }
    "Document.Title=$($Document.Title);"
    "Document.Metadata=$($Document.Metadata['author']);"
}

# Synopsis: Test $LocalizedData variable
Document 'PSDocsLocalizedDataVariable' {
    "LocalizedData.Key1=$($LocalizedData.Key1);"
}
