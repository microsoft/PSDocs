
# Define a test document with a table
document 'WithExternalScript' {
    $InputObject.ResourceType.WindowsFeature | Where-Object { $_.Ensure -eq 'Absent' } | Table -Property Name;
}
