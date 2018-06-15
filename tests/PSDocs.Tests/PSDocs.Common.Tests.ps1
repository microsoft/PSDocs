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

Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs") -Force;
Import-Module (Join-Path -Path $rootPath -ChildPath "out/modules/PSDocs/PSDocsProcessor/Markdown") -Force;

$outputPath = "$temp\PSDocs.Tests\Common";
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction SilentlyContinue;
New-Item -Path $outputPath -ItemType Directory -Force | Out-Null;

$dummyObject = New-Object -TypeName PSObject -Property @{
    Object = [PSObject]@{
        Name = 'ObjectName'
        Value = 'ObjectValue'
    }

    Hashtable = @{
        Name = 'HashName'
        Value = 'HashValue'
    }
}

$Global:TestVars = @{ };

Describe 'PSDocs instance names' {
    Context 'Generate a document without an instance name' {

        # Define a test document with a table
        document 'WithoutInstanceName' {
            $InstanceName;

            $InputObject.Object.Name;

            $InputObject.Hashtable.Name;
        }

        $outputDoc = "$outputPath\WithoutInstanceName.md";
        WithoutInstanceName -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should generate an output named WithoutInstanceName.md' {
            Test-Path -Path $outputDoc | Should be $True;
        }

        It 'Should contain document name' {
            Get-Content -Path $outputDoc -Raw | Should -Match 'WithoutInstanceName';
        }

        It 'Should contain object properties' {
            Get-Content -Path $outputDoc -Raw | Should -Match 'ObjectName';
            Get-Content -Path $outputDoc -Raw | Should -Match 'HashName';
        }
    }

    Context 'Generate a document with an instance name' {
        
        # Define a test document with a table
        document 'WithInstanceName' {
            $InstanceName;
        }

        WithInstanceName -InstanceName 'Instance1' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should not create a output with the document name' {
            Test-Path -Path "$outputPath\WithInstanceName.md" | Should be $False;
        }

        It 'Should generate an output named Instance1.md' {
            Test-Path -Path "$outputPath\Instance1.md" | Should be $True;
        }

        It 'Should contain instance name' {
            Get-Content -Path "$outputPath\Instance1.md" -Raw | Should match 'Instance1';
        }
    }

    Context 'Generate a document with multiple instance names' {
        
        # Define a test document with a table
        document 'WithMultiInstanceName' {
            $InstanceName;
        }

        WithMultiInstanceName -InstanceName 'Instance2','Instance3' -InputObject $dummyObject -OutputPath $outputPath;

        It 'Should not create a output with the document name' {
            Test-Path -Path "$outputPath\WithMultiInstanceName.md" | Should be $False;
        }

        It 'Should generate an output named Instance2.md' {
            Test-Path -Path "$outputPath\Instance2.md" | Should be $True;
        }

        It 'Should contain instance name Instance2' {
            Get-Content -Path "$outputPath\Instance2.md" -Raw | Should match 'Instance2';
        }

        It 'Should generate an output named Instance3.md' {
            Test-Path -Path "$outputPath\Instance3.md" | Should be $True;
        }

        It 'Should contain instance name Instance3' {
            Get-Content -Path "$outputPath\Instance3.md" -Raw | Should match 'Instance3';
        }
    }

    Context 'Generate a document with a specific encoding' {

        document 'WithEncoding' {
            $InstanceName;
        }

        # Check each encoding can be written then read
        foreach ($encoding in @('UTF8', 'UTF7', 'Unicode', 'ASCII', 'UTF32')) {

            It "Should generate $encoding encoded content" {
                WithEncoding -InstanceName "With$encoding" -InputObject $dummyObject -OutputPath $outputPath -Encoding $encoding;
                Get-Content -Path (Join-Path -Path $outputPath -ChildPath "With$encoding.md") -Raw -Encoding $encoding | Should -BeExactly "With$encoding`r`n";
            }
        }
    }
}

Describe 'Invoke-PSDocument' -Tag 'FromPath' {
    Context 'With -Path' {

        # Only generate documents for the named document
        Invoke-PSDocument -Path $here -OutputPath $outputPath -Name 'FromFileTest2' -Verbose;

        It 'Should generate named documents' {
            Test-Path -Path "$outputPath\FromFileTest1.md" | Should be $False;
            Test-Path -Path "$outputPath\FromFileTest2.md" | Should be $True;
            Test-Path -Path "$outputPath\FromFileTest3.md" | Should be $False;
        }

        Invoke-PSDocument -Path $here -OutputPath $outputPath -Tag 'Test3' -Verbose;

        It 'Should generate tagged documents' {
            Test-Path -Path "$outputPath\FromFileTest1.md" | Should be $False;
            Test-Path -Path "$outputPath\FromFileTest3.md" | Should be $True;
        }

        Invoke-PSDocument -Path $here -OutputPath $outputPath -Tag 'Test4','Test5' -Verbose;

        It 'Should generate tagged documents' {
            Test-Path -Path "$outputPath\FromFileTest1.md" | Should be $False;
            Test-Path -Path "$outputPath\FromFileTest4.md" | Should be $False;
            Test-Path -Path "$outputPath\FromFileTest5.md" | Should be $True;
        }
    }
}
