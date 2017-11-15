
[CmdletBinding()]
param (
    [Switch]$Scaffold = $False
)

if ($Scaffold) {
    Import-Module '.\src\PSDocs' -Force;
    Import-Module '.\src\PSDocs.Dsc' -Force;

    Update-MarkdownHelp -Path '.\docs\commands\PSDocs\en-US';
    Update-MarkdownHelp -Path '.\docs\commands\PSDocs.Dsc\en-US';
}

New-ExternalHelp -OutputPath '.\build\docs' -Path '.\docs\commands\PSDocs\en-US' -Force;
New-ExternalHelp -OutputPath '.\build\docs' -Path '.\docs\commands\PSDocs.Dsc\en-US' -Force;