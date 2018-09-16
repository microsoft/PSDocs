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

Import-Module (Join-Path -Path $rootPath -ChildPath 'out/modules/PSDocs') -Force;

$outputPath = "$temp\PSDocs.Tests\Common";
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction SilentlyContinue;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;

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

Describe 'PSDocs instance names' -Tag Common {
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
            $outputDoc | Should -FileContentMatch 'WithoutInstanceName';
        }

        It 'Should contain object properties' {
            $outputDoc | Should -FileContentMatch 'ObjectName';
            $outputDoc | Should -FileContentMatch 'HashName';
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

        WithMultiInstanceName -InstanceName 'Instance2','Instance3' -InputObject @{} -OutputPath $outputPath;

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
                WithEncoding -InstanceName "With$encoding" -InputObject @{} -OutputPath $outputPath -Encoding $encoding;
                Get-Content -Path (Join-Path -Path $outputPath -ChildPath "With$encoding.md") -Raw -Encoding $encoding | Should -BeExactly "With$encoding`r`n";
            }
        }
    }
}

Describe 'Invoke-PSDocument' -Tag 'FromPath' {
    Context 'With -Path' {

        It 'Should match name' {
            # Only generate documents for the named document
            Invoke-PSDocument -Path $here -OutputPath $outputPath -Name FromFileTest2;
            Test-Path -Path "$outputPath\FromFileTest1.md" | Should be $False;
            Test-Path -Path "$outputPath\FromFileTest2.md" | Should be $True;
            Test-Path -Path "$outputPath\FromFileTest3.md" | Should be $False;
        }

        It 'Should match single tag' {
            # Only generate for documents with matching tag
            Invoke-PSDocument -Path $here -OutputPath $outputPath -Tag Test3;
            Test-Path -Path "$outputPath\FromFileTest1.md" | Should be $False;
            Test-Path -Path "$outputPath\FromFileTest3.md" | Should be $True;
        }

        It 'Should match all tags' {
            # Only generate for documents with all matching tags
            Invoke-PSDocument -Path $here -OutputPath $outputPath -Tag Test4,Test5;
            Test-Path -Path "$outputPath\FromFileTest1.md" | Should -Be $False;
            Test-Path -Path "$outputPath\FromFileTest4.md" | Should -Be $False;
            Test-Path -Path "$outputPath\FromFileTest5.md" | Should -Be $True;
        }

        It 'Should generate exception' {
            { Invoke-PSDocument -Path $here -OutputPath $outputPath -Name InvalidCommand -ErrorAction Stop } | Should -Throw -ExceptionType PSDocs.Execution.InvokeDocumentException;
            $Error[0].Exception.Message | Should -Match '^(The term ''New-PSDocsInvalidCommand'' is not recognized as the name of a cmdlet)';
            { Invoke-PSDocument -Path $here -OutputPath $outputPath -Name InvalidCommandWithSection -ErrorAction Stop } | Should -Throw -ExceptionType PSDocs.Execution.InvokeDocumentException;
            $Error[0].Exception.Message | Should -Match '^(The term ''New-PSDocsInvalidCommand'' is not recognized as the name of a cmdlet)';
        }
    }

    Context 'With constrained language' {

        It 'Checks if DeviceGuard is enabled' {
            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSDocs -Verifiable -MockWith {
                return $True;
            }

            Invoke-PSDocument -Path $here -OutputPath $outputPath -Name 'ConstrainedTest1';
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSDocs -Times 1;
        }

        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            { Invoke-PSDocument -Path $here -OutputPath $outputPath -Name 'ConstrainedTest2' -Option @{ 'execution.mode' = 'ConstrainedLanguage' } -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
            Test-Path -Path "$outputPath\ConstrainedTest2.md" | Should -Be $False;
        }
    }
}

Describe 'New-PSDocumentOption' -Tag 'Option' {

    Context 'Read PSDocs.yml' {

        It 'can read default YAML' {
            $option = New-PSDocumentOption;
            $option.Generator | Should -Be 'PSDocs';
        }
    }

    Context 'Read Markdown.Encoding' {

        It 'from default' {
            $option = New-PSDocumentOption;
            $option.Markdown.Encoding | Should -Be Default;
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Markdown.Encoding' = 'UTF8' };
            $option.Markdown.Encoding | Should -Be 'UTF8';
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Markdown.Encoding | Should -Be 'UTF8';
        }
    }

    Context 'Read Markdown.WrapSeparator' {

        It 'from default' {
            $option = New-PSDocumentOption;
            $option.Markdown.WrapSeparator | Should -Be ' ';
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Markdown.WrapSeparator' = 'ZZZ' };
            $option.Markdown.WrapSeparator | Should -Be 'ZZZ';
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Markdown.WrapSeparator | Should -Be 'ZZZ';
        }
    }

    Context 'Read Markdown.SkipEmptySections' {

        It 'from default' {
            $option = New-PSDocumentOption;
            $option.Markdown.SkipEmptySections | Should -Be $True;
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Markdown.SkipEmptySections' = $False };
            $option.Markdown.SkipEmptySections | Should -Be $False;
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Markdown.SkipEmptySections | Should -Be $False;
        }
    }

    Context 'Read Execution.LanguageMode' {

        It 'from default' {
            $option = New-PSDocumentOption;
            $option.Execution.LanguageMode | Should -Be FullLanguage;
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' };
            $option.Execution.LanguageMode | Should -Be ConstrainedLanguage;
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Execution.LanguageMode | Should -Be ConstrainedLanguage
        }
    }
}
