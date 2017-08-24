
# Define a test document with a table
document 'WithExternalScript' {
    $InputObject.ResourceType.File | Table -Property Contents,DestinationPath;
}
