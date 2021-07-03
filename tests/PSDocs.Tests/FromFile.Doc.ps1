# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

Export-PSDocumentConvention 'FromFileTest1' {

}

Document 'FromFileTest1' {
    Title 'Test title'
    Metadata @{
        test = 'Test1'
    }
    Section 'Test' {
        'Test 1'
    }
}

Document 'FromFileTest2' {
    Metadata @{
        test = 'Test2'
    }
    Section 'Test' {
        'Test 2'
    }
}

Document 'FromFileTest3' -Tag 'Test3' {
    Metadata @{
        test = 'Test3'
    }
    Section 'Test' {
        'Test 3'
    }
}

Document 'FromFileTest4' -Tag 'Test4' {
    Section 'Test' {
        'Test 4'
    }
}

Document 'FromFileTest5' -Tag 'Test4','Test5' {
    Section 'Test' {
        'Test 5'
    }
}

Document 'ConstrainedTest1' {
    Section 'Test' {
        'Test 1'
    }
}

Document 'ConstrainedTest2' {

    [Console]::WriteLine('Should fail');
}

Document 'WithIf' -If { $TargetObject.Generator -eq 'PSDocs' } {
    Metadata @{
        Name = $PSDocs.TargetObject.Name
    }

    'EOF'
}

Document 'WithSelector' -With 'GeneratorSelector' {
    Metadata @{
        Name = $PSDocs.TargetObject.Name
    }

    'EOF'
}
