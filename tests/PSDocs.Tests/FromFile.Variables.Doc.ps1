
# Synopsis: Document for unit testing
Document 'PSAutomaticVariables' {
    "PWD=$PWD;"
    "PSScriptRoot=$PSScriptRoot;"
    "PSCommandPath=$PSCommandPath;"
}

# Synopsis: Test $PSDocs variable
Document 'PSDocsVariable' {
    Metadata @{
        author = $PSDocs.Configuration.author.name
    }
    $PSDocs.Format("TargetObject.Name={0};", $PSDocs.TargetObject.Name);
    if ($PSDocs.Configuration.GetBoolOrDefault('NotConfig', $True)) {
        "Document.Metadata=$($Document.Metadata['author']);"
    }
    if ($PSDocs.Configuration.GetBoolOrDefault('Enabled', $True)) {
        "Document.Enabled=true"
    }
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
