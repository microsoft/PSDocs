#
# Unit tests for the Warning keyword
#

[CmdletBinding()]
param (

)

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

# Setup tests paths
$rootPath = (Resolve-Path $PSScriptRoot\..\..).Path;
$here = Split-Path -Parent $MyInvocation.MyCommand.Path;
$temp = "$here\..\..\build";

Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs") -Force;
Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs/PSDocsProcessor/Markdown") -Force;

$outputPath = "$temp\PSDocs.Tests\Warning";
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction SilentlyContinue;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

$dummyObject = New-Object -TypeName PSObject;

$Global:TestVars = @{ };

Describe 'PSDocs -- Warning keyword' {

    Context 'Warning single line markdown' {
        
        # Define a test document with a warning
        document 'WarningSingleMarkdown' {
            
            Warning {
                'This is a single line warning'
            }
        }

        $outputDoc = "$outputPath\WarningSingleMarkdown.md";
        WarningSingleMarkdown -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '\> \[\!WARNING\]\r\n\> This is a single line warning';
        }
    }

    Context 'Warning multi-line markdown' {
        
        # Define a test document with a warning
        document 'WarningMultiMarkdown' {
            
            Warning {
                'This is the first line of the warning.'
                'This is the second line of the warning.'
            }
        }

        $outputDoc = "$outputPath\WarningMultiMarkdown.md";
        WarningMultiMarkdown -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should have generated output' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should match expected format' {
            Get-Content -Path $outputDoc -Raw | Should match '\> \[\!WARNING\]\r\n\> This is the first line of the warning.\r\n\> This is the second line of the warning.';
        }
    }
}
