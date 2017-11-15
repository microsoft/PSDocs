
[CmdletBinding()]
param (
    [Switch]$Scaffold = $False`
)

Import-Module '.\src\PSDocs' -Force;
Import-Module '.\src\PSDocs.Dsc' -Force;


if ($Scaffold) {

    Update-MarkdownHelp -Path '.\docs\commands\PSDocs\en-US';
    Update-MarkdownHelp -Path '.\docs\commands\PSDocs.Dsc\en-US';
}