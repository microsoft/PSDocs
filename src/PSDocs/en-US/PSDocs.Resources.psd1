# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

ConvertFrom-StringData @'
###PSLOC
DocumentNotFound=Failed to find document: {0}
PathNotFound=Path not found
SourceNotFound=Could not find any .Doc.ps1 script files in the path.
DocumentProcessFailure=Failed to process document
SectionProcessFailure=Failed to process section: {0}
KeywordOutsideEngine=This keyword can only be called within PSDocs. Add document definitions to .Doc.ps1 files, then execute them with Invoke-PSDocument.
###PSLOC
'@
