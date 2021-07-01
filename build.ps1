# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Note:
# This manually builds the project locally

. ./.azure-pipelines/pipeline-deps.ps1
Invoke-Build Test

Write-Host 'If no build errors occurred. The module has been saved to out/modules/PSDocs'
