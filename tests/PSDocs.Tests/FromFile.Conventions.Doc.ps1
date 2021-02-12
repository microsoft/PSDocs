#
# Conventions for unit testing
#

# Synopsis: An example naming convention.
Export-PSDocumentConvention 'TestNamingConvention1' {
    Write-Verbose -Message 'Convention process'
    $PSDocs.Document.InstanceName = [String]::Concat($PSDocs.Document.Data.testName, '_', $PSDocs.Document.Data.count);
    $newPath = Join-Path -Path $PSDocs.Document.OutputPath -ChildPath 'new';
    $PSDocs.Document.OutputPath = $newPath;
} -Begin {
    Write-Verbose -Message 'Convention begin';
    $PSDocs.Document.Data['count'] = 1;
} -End {
    Write-Verbose -Message "Convention end";
    foreach ($output in $PSDocs.Output) {
        $tocPath = Join-Path -Path $output.OutputPath -ChildPath 'toc.yaml';
        Set-Content -Path $tocPath -Value '';
    }
}

# Synopsis: An example naming convention.
Export-PSDocumentConvention 'TestNamingConvention2' {
    $PSDocs.Document.Data.count += 1;
    Write-Verbose -Message 'Convention process'
    $PSDocs.Document.InstanceName = [String]::Concat($PSDocs.Document.Data.testName, '_', $PSDocs.Document.Data.count);
    $newPath = Join-Path -Path $PSDocs.Document.OutputPath -ChildPath 'new';
    $PSDocs.Document.OutputPath = $newPath;
}

# Synopsis: A test document for testing conventions.
Document 'ConventionDoc1' {
    $PSDocs.Document.Data.testName = $PSDocs.TargetObject.Name;
    $PSDocs.Document.Data.count += 1;
    'Convention test'
}
