#
# Document definitions for unit testing
#

# Synopsis: Document for unit testing
Document 'WithoutInstanceName' {
    $InstanceName;
    $InputObject.Object.Name;
    $InputObject.Hashtable.Name;
}

# Synopsis: Document for unit testing
Document 'WithInstanceName' {
    $InstanceName;
}

# Synopsis: Document for unit testing
document 'WithMultiInstanceName' {
    $InstanceName;
}

# Synopsis: Document for unit testing
document 'WithEncoding' {
    $InstanceName;
}

# Synopsis: Document for unit testing
document 'WithPassThru' {
    $InstanceName;
}

# Synopsis: Document for unit testing
document 'WithMetadata' {
    Metadata @{
        key1 = 'value1'
    }
    Section 'Test' -Force {
    }
}
