
[CmdletBinding()]
param (
    [Switch]$Scaffold = $False
)

if ($Scaffold) {
    Import-Module '.\src\PSDocs' -Force;
    Import-Module '.\src\PSDocs.Dsc' -Force;

    Update-MarkdownHelp -Path '.\docs\commands\PSDocs\en-US';
    Update-MarkdownHelp -Path '.\docs\commands\PSDocs.Dsc\en-US';

    return;
}

New-ExternalHelp -OutputPath '.\build\docs' -Path '.\docs\commands\PSDocs\en-US' -Force;
New-ExternalHelp -OutputPath '.\build\docs' -Path '.\docs\commands\PSDocs.Dsc\en-US' -Force;

Copy-Item -Path '.\build\docs\PSDocs-help.xml' -Destination '.\src\PSDocs\en-US';
Copy-Item -Path '.\build\docs\PSDocs-help.xml' -Destination '.\src\PSDocs\en-AU';
Copy-Item -Path '.\build\docs\PSDocs.Dsc-help.xml' -Destination '.\src\PSDocs.Dsc\en-US';
Copy-Item -Path '.\build\docs\PSDocs.Dsc-help.xml' -Destination '.\src\PSDocs.Dsc\en-AU';