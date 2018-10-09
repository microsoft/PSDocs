#
# Unit tests for the Include keyword
#

[CmdletBinding()]
param (

)

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

# Setup tests paths
$rootPath = $PWD;

Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;

$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSDocs.Tests/Include;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

Describe 'PSDocs -- Include keyword' -Tag Include {
    Context 'Markdown' {

        It 'Should include a relative path' {
            document 'IncludeRelative' {
                Include tests/PSDocs.Tests/IncludeFile.md
                Include IncludeFile2.md -BaseDirectory tests/PSDocs.Tests/
            }

            $outputDoc = "$outputPath\IncludeRelative.md";
            IncludeRelative -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should -Be $True;
            $outputDoc | Should -FileContentMatch 'This is included from an external file.';
            $outputDoc | Should -FileContentMatch 'This is a second file to include.';
        }

        It 'Should include an absolute path' {
            document 'IncludeAbsolute' {
                Include (Join-Path -Path $PWD -ChildPath tests/PSDocs.Tests/IncludeFile.md)
            }

            $outputDoc = "$outputPath\IncludeAbsolute.md";
            IncludeAbsolute -OutputPath $outputPath;

            Test-Path -Path $outputDoc | Should -Be $True;
            $outputDoc | Should -FileContentMatch 'This is included from an external file.';
        }

        It 'Should include from culture' {
            document 'IncludeCulture' {
                Include IncludeFile3.md -UseCulture -BaseDirectory tests/PSDocs.Tests/
            }

            IncludeCulture -OutputPath $outputPath -Culture 'en-AU','en-US' -Verbose;

            $outputDoc = "$outputPath\en-AU\IncludeCulture.md";
            Test-Path -Path $outputDoc | Should -Be $True;
            $outputDoc | Should -FileContentMatch 'This is en-AU.';

            $outputDoc = "$outputPath\en-US\IncludeCulture.md";
            Test-Path -Path $outputDoc | Should -Be $True;
            $outputDoc | Should -FileContentMatch 'This is en-US.';
        }
    }
}
