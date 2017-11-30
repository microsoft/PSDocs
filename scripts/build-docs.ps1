
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

# Generate external help files
New-ExternalHelp -OutputPath '.\build\docs\PSDocs' -Path '.\docs\commands\PSDocs\en-US','.\docs\keywords\PSDocs\en-US' -Force;
New-ExternalHelp -OutputPath '.\build\docs\PSDocs.Dsc' -Path '.\docs\commands\PSDocs.Dsc\en-US' -Force;

# Clean up old external help files
Remove-Item -Path '.\src\PSDocs\**\*.help.txt';
Remove-Item -Path '.\src\PSDocs\**\*-help.xml';
Remove-Item -Path '.\src\PSDocs.Dsc\**\*.help.txt';
Remove-Item -Path '.\src\PSDocs.Dsc\**\*-help.xml';

# Coyp new external help files
Copy-Item -Path '.\build\docs\PSDocs\*.*' -Destination '.\src\PSDocs\en-US';
Copy-Item -Path '.\build\docs\PSDocs\*.*' -Destination '.\src\PSDocs\en-AU';
Copy-Item -Path '.\build\docs\PSDocs.Dsc\*.*' -Destination '.\src\PSDocs.Dsc\en-US';
Copy-Item -Path '.\build\docs\PSDocs.Dsc\*.*' -Destination '.\src\PSDocs.Dsc\en-AU';