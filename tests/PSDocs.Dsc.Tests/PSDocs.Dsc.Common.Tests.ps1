#
# Unit tests for core PSDocs functionality
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
# $sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.';

Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs") -Force;
Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs.Dsc") -Force;

$outputPath = "$temp\PSDocs.Dsc.Tests\Common";
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction SilentlyContinue;
New-Item -Path $outputPath -ItemType Directory -Force | Out-Null;

$Global:TestVars = @{ };

configuration TestConfiguration {

    param (
        [Parameter(Mandatory = $True)]
        [String[]]$ComputerName
    )

    Import-DscResource -ModuleName PSDesiredStateConfiguration;

    node $ComputerName {

        File FileResource {
            Ensure = 'Present'
            Type = 'File'
            DestinationPath = 'C:\environment.tag'
            Contents = "Node=$($Node.NodeName)"
        }

        WindowsFeature SMB1 {
            Ensure = 'Absent'
            Name = 'FS-SMB1'
        }
    }
}

configuration TestConfiguration2 {
    
    param (
        [Parameter(Mandatory = $True)]
        [String[]]$ComputerName
    )

    Import-DscResource -ModuleName PSDesiredStateConfiguration;

    node $ComputerName {

        File FileResource {
            Ensure = 'Present'
            Type = 'File'
            DestinationPath = 'C:\environment.tag'
            Contents = "Node=$($Node.NodeName)"
        }
    }
}

Describe 'PSDocs.Dsc' -Tag 'Dsc' {
    Context 'Generate a document without an instance name' {

        # Define a test document with a table
        document 'WithoutInstanceName' {
            $InputObject.ResourceType.File | Table -Property Contents,DestinationPath;
        }

        TestConfiguration -OutputPath $outputPath -ComputerName 'WithoutInstanceName';

        $outputDoc = "$outputPath\WithoutInstanceName.md";
        Invoke-DscNodeDocument -DocumentName 'WithoutInstanceName' -Path $outputPath -OutputPath $outputPath;

        It 'Should generate an output named WithoutInstanceName.md' {
            Test-Path -Path $outputDoc | Should -Be $True;
        }

        It 'Should contain document name' {
            $outputDoc | Should -FileContentMatch '\|Node\=WithoutInstanceName\|';
        }
    }

    Context 'Generate a document with an instance name' {
        
        # Define a test document with a table
        document 'WithInstanceName' {
            $InputObject.ResourceType.File | Table -Property Contents,DestinationPath;
        }

        TestConfiguration -OutputPath $outputPath -ComputerName 'Instance1';

        Invoke-DscNodeDocument -DocumentName 'WithInstanceName' -InstanceName 'Instance1' -Path $outputPath -OutputPath $outputPath;

        It 'Should not create a output with the document name' {
            Test-Path -Path "$outputPath\WithInstanceName.md" | Should -Be $False;
        }

        It 'Should generate an output named Instance1.md' {
            Test-Path -Path "$outputPath\Instance1.md" | Should -Be $True;
        }

        It 'Should contain instance name' {
            "$outputPath\Instance1.md" | Should -FileContentMatch '|Content=Instance1|';
        }
    }

    Context 'Generate a document with multiple instance names' {
        
        # Define a test document with a table
        document 'WithMultiInstanceName' {
            $InputObject.ResourceType.File | Table -Property Contents,DestinationPath;
        }

        TestConfiguration -OutputPath $outputPath -ComputerName 'Instance2','Instance3';

        Invoke-DscNodeDocument -DocumentName 'WithMultiInstanceName' -InstanceName 'Instance2','Instance3' -Path $outputPath -OutputPath $outputPath;

        It 'Should not create a output with the document name' {
            Test-Path -Path "$outputPath\WithMultiInstanceName.md" | Should -Be $False;
        }

        It 'Should generate an output named Instance2.md' {
            Test-Path -Path "$outputPath\Instance2.md" | Should -Be $True;
        }

        It 'Should contain instance name Instance2' {
            "$outputPath\Instance2.md" | Should -FileContentMatch '\|Node\=Instance2\|';
        }

        It 'Should generate an output named Instance3.md' {
            Test-Path -Path "$outputPath\Instance3.md" | Should -Be $True;
        }

        It 'Should contain instance name Instance3' {
            "$outputPath\Instance3.md" | Should -FileContentMatch '\|Node\=Instance3\|';
        }
    }

    Context 'Generate a document with an external script' {

        TestConfiguration -OutputPath $outputPath -ComputerName 'WithExternalScript';

        Invoke-DscNodeDocument -Script "$here\Templates\WithExternalScript.ps1" -DocumentName 'WithExternalScript' -InstanceName 'WithExternalScript' -Path $outputPath -OutputPath $outputPath;

        It 'Should generate an output named WithExternalScript.md' {
            Test-Path -Path "$outputPath\WithExternalScript.md" | Should -Be $True;
        }

        It 'Should contain instance name' {
            "$outputPath\WithExternalScript.md" | Should -FileContentMatch '\|FS\-SMB1\|';
        }
    }

    Context 'Generate a document with missing data' {
        
        # Define a test document with a table
        document 'WithMissingData' {

            Section 'Windows features' {
                # Reference a resource type that is not included in the configuration
                $InputObject.ResourceType.WindowsFeature | Table -Property Name,Ensure;
            }
        }

        TestConfiguration2 -OutputPath $outputPath -ComputerName 'WithMissingData';

        Invoke-DscNodeDocument -DocumentName 'WithMissingData' -InstanceName 'WithMissingData' -Path $outputPath -OutputPath $outputPath;

        It 'Should output' {
            Test-Path -Path "$outputPath\WithMissingData.md" | Should -Be $True;
        }
    }

    Context 'Generate a document with encoding' {

        document 'EncodingTest' {
            $PSDocs.Option.Markdown.Encoding
        }

        TestConfiguration2 -OutputPath $outputPath -ComputerName 'EncodingTest';

        Invoke-DscNodeDocument -DocumentName 'EncodingTest' -InstanceName 'EncodingTest' -Encoding 'UTF8' -Path $outputPath -OutputPath $outputPath;

        It 'Called Invoke-PSDocument with Encoding' {
            (Join-Path -Path $outputPath -ChildPath 'EncodingTest.md') | Should -FileContentMatch 'UTF8';
        }
    }
}
