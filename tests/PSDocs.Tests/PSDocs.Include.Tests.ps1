# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for the Include keyword
#

[CmdletBinding()]
param ()

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

# Setup tests paths
$rootPath = $PWD;

Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;

$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSDocs.Tests/Include;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;
$here = (Resolve-Path $PSScriptRoot).Path;

Describe 'PSDocs -- Include keyword' -Tag Include {
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Keyword.Doc.ps1';
    $testObject = [PSCustomObject]@{};

    Context 'Markdown' {
        $invokeParams = @{
            Path = $docFilePath
            OutputPath = $outputPath
            ErrorAction = [System.Management.Automation.ActionPreference]::Stop
        }

        It 'Should include a relative path' {
            $outputDoc = "$outputPath\IncludeRelative.md";
            $Null = Invoke-PSDocument @invokeParams -InputObject $rootPath -Name 'IncludeRelative';

            Test-Path -Path $outputDoc | Should -Be $True;
            $outputDoc | Should -FileContentMatch 'This is included from an external file.';
            $outputDoc | Should -FileContentMatch 'This is a second file to include.';
        }

        It 'Should include an absolute path' {
            $outputDoc = "$outputPath\IncludeAbsolute.md";
            $Null = Invoke-PSDocument @invokeParams -InputObject $rootPath -Name 'IncludeAbsolute';

            Test-Path -Path $outputDoc | Should -Be $True;
            $outputDoc | Should -FileContentMatch 'This is included from an external file.';
        }

        It 'Should include from culture' {
            $Null = $testObject | Invoke-PSDocument @invokeParams -Culture 'en-AU','en-US' -Name 'IncludeCulture';

            $outputDoc = "$outputPath\en-AU\IncludeCulture.md";
            Test-Path -Path $outputDoc | Should -Be $True;
            $outputDoc | Should -FileContentMatch 'This is en-AU.';

            $outputDoc = "$outputPath\en-US\IncludeCulture.md";
            Test-Path -Path $outputDoc | Should -Be $True;
            $outputDoc | Should -FileContentMatch 'This is en-US.';
        }

        It 'Should include when file exists' {
            $Null = $testObject | Invoke-PSDocument @invokeParams -Name 'IncludeOptional';
            { $Null = $testObject | Invoke-PSDocument @invokeParams -Name 'IncludeRequired'; } | Should -Throw -Because 'PSDocs.Runtime.IncludeNotFound';
        }

        It 'Should replace tokens' {
            $result = $testObject | Invoke-PSDocument @invokeParams -Name 'IncludeReplace' -PassThru;
            $result | Should -BeLike 'This is a third file to include.*'
        }
    }
}
