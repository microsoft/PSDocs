
document 'FromFileTest1' {
    Title 'Test title'
    Metadata @{
        test = 'Test1'
    }
    Section 'Test' {
        'Test 1'
    }
}

document 'FromFileTest2' {
    Metadata @{
        test = 'Test2'
    }
    Section 'Test' {
        'Test 2'
    }
}

document 'FromFileTest3' -Tag 'Test3' {
    Metadata @{
        test = 'Test3'
    }
    Section 'Test' {
        'Test 3'
    }
}

document 'FromFileTest4' -Tag 'Test4' {
    Section 'Test' {
        'Test 4'
    }
}

document 'FromFileTest5' -Tag 'Test4','Test5' {
    Section 'Test' {
        'Test 5'
    }
}

document 'ConstrainedTest1' {
    Section 'Test' {
        'Test 1'
    }
}

document 'ConstrainedTest2' {

    [Console]::WriteLine('Should fail');
}
