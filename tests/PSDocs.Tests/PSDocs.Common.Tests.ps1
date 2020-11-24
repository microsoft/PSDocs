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
$rootPath = $PWD;

Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;

$outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSDocs.Tests/Common;
Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
$Null = New-Item -Path $outputPath -ItemType Directory -Force;
$here = (Resolve-Path $PSScriptRoot).Path;

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

Describe 'PSDocs instance names' -Tag 'Common', 'InstanceName' {
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Cmdlets.Doc.ps1';

    Context 'Generate a document without an instance name' {
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $dummyObject
            OutputPath = $outputPath
        }
        $result = Invoke-PSDocument @invokeParams -Name 'WithoutInstanceName';
        It 'Should generate an output named WithoutInstanceName.md' {
            Test-Path -Path $result.FullName | Should be $True;
            $outputDoc = Get-Content -Path $result.FullName -Raw;
            $outputDoc | Should -Match 'WithoutInstanceName';
            $outputDoc | Should -Match 'ObjectName';
            $outputDoc | Should -Match 'HashName';
        }
    }

    Context 'Generate a document with an instance name' {
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $dummyObject
            OutputPath = $outputPath
        }
        $result = Invoke-PSDocument @invokeParams -InstanceName 'Instance1' -Name 'WithInstanceName';
        It 'Should not create a output with the document name' {
            Test-Path -Path "$outputPath\WithInstanceName.md" | Should -Be $False;
            Test-Path -Path "$outputPath\Instance1.md" | Should -Be $True;
            Get-Content -Path "$outputPath\Instance1.md" -Raw | Should -Match 'Instance1';
        }
    }

    Context 'Generate a document with multiple instance names' {
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $dummyObject
            OutputPath = $outputPath
        }
        $result = Invoke-PSDocument @invokeParams -InstanceName 'Instance2','Instance3' -Name 'WithMultiInstanceName';
        It 'Should not create a output with the document name' {
            Test-Path -Path "$outputPath\WithMultiInstanceName.md" | Should be $False;
        }
        It 'Should generate an output named Instance2.md' {
            Test-Path -Path "$outputPath\Instance2.md" | Should be $True;
            Get-Content -Path "$outputPath\Instance2.md" -Raw | Should match 'Instance2';
        }
        It 'Should generate an output named Instance3.md' {
            Test-Path -Path "$outputPath\Instance3.md" | Should be $True;
            Get-Content -Path "$outputPath\Instance3.md" -Raw | Should match 'Instance3';
        }
    }

    Context 'Generate a document with a specific encoding' {
        $testObject = [PSCustomObject]@{
            Name = 'TestObject'
        }
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $testObject
            OutputPath = $outputPath
        }
        # Check each encoding can be written then read
        foreach ($encoding in @('UTF8', 'UTF7', 'Unicode', 'ASCII', 'UTF32')) {
            It "Should generate $encoding encoded content" {
                Invoke-PSDocument @invokeParams -InstanceName "With$encoding" -Encoding $encoding -Name 'WithEncoding';
                Get-Content -Path (Join-Path -Path $outputPath -ChildPath "With$encoding.md") -Encoding $encoding | Out-String | Should -Match "^(With$encoding(\r|\n|\r\n))$";
            }
        }
    }

    Context 'With -PassThru' {
        $testObject = [PSCustomObject]@{
            Name = 'TestObject'
        }
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $testObject
            PassThru = $True
        }
        It 'Should return results' {
            $result = Invoke-PSDocument @invokeParams -Name 'WithPassThru';
            $result | Should -Match 'WithPassThru';
        }
    }
}

Describe 'Invoke-PSDocument' -Tag 'Cmdlet', 'Common', 'Invoke-PSDocument', 'FromPath' {
    Context 'With -Path' {
        It 'Should match name' {
            # Only generate documents for the named document
            Invoke-PSDocument -Path $here -OutputPath $outputPath -Name FromFileTest2;
            Test-Path -Path "$outputPath\FromFileTest1.md" | Should -Be $False;
            Test-Path -Path "$outputPath\FromFileTest2.md" | Should -Be $True;
            Test-Path -Path "$outputPath\FromFileTest3.md" | Should -Be $False;
        }
        It 'Should match single tag' {
            # Only generate for documents with matching tag
            Invoke-PSDocument -Path $here -OutputPath $outputPath -Tag Test3;
            Test-Path -Path "$outputPath\FromFileTest1.md" | Should -Be $False;
            Test-Path -Path "$outputPath\FromFileTest3.md" | Should -Be $True;
        }
        It 'Should match all tags' {
            # Only generate for documents with all matching tags
            Invoke-PSDocument -Path $here -OutputPath $outputPath -Tag Test4,Test5;
            Test-Path -Path "$outputPath\FromFileTest1.md" | Should -Be $False;
            Test-Path -Path "$outputPath\FromFileTest4.md" | Should -Be $False;
            Test-Path -Path "$outputPath\FromFileTest5.md" | Should -Be $True;
        }
        It 'Should generate exception' {
            { Invoke-PSDocument -Path $here -OutputPath $outputPath -Name InvalidCommand -ErrorAction Stop } | Should -Throw -ExceptionType PSDocs.Pipeline.RuntimeException;
            $Error[0].Exception.Message | Should -Match '^(The term ''New-PSDocsInvalidCommand'' is not recognized)';
            { Invoke-PSDocument -Path $here -OutputPath $outputPath -Name InvalidCommandWithSection -ErrorAction Stop } | Should -Throw -ExceptionType PSDocs.Pipeline.RuntimeException;
            $Error[0].Exception.Message | Should -Match '^(The term ''New-PSDocsInvalidCommand'' is not recognized)';
            { Invoke-PSDocument -Path $here -OutputPath $outputPath -Name WithWriteError -ErrorAction Stop } | Should -Throw;
            $Error[0].Exception.Message | Should -Match 'Verify Write-Error is raised as an exception';
        }
    }

    Context 'With -PassThru' {
        It 'Should return results' {
            $result = @(Invoke-PSDocument -Path $here -OutputPath $outputPath -Name FromFileTest1,FromFileTest2 -PassThru);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0] | Should -Match "`# Test title";
            $result[1] | Should -Match "test: Test2";
        }
    }

    Context 'With constrained language' {
        # Check that '[Console]::WriteLine('Should fail')' is not executed
        It 'Should fail to execute blocked code' {
            { Invoke-PSDocument -Path $here -OutputPath $outputPath -Name 'ConstrainedTest2' -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' } -ErrorAction Stop } | Should -Throw 'Cannot invoke method. Method invocation is supported only on core types in this language mode.';
            Test-Path -Path "$outputPath\ConstrainedTest2.md" | Should -Be $False;
        }
        It 'Checks if DeviceGuard is enabled' {
            Mock -CommandName IsDeviceGuardEnabled -ModuleName PSDocs -Verifiable -MockWith {
                return $True;
            }
            Invoke-PSDocument -Path $here -OutputPath $outputPath -Name 'ConstrainedTest1';
            Assert-MockCalled -CommandName IsDeviceGuardEnabled -ModuleName PSDocs -Times 1;
        }
    }
}

Describe 'Get-PSDocument' -Tag 'Cmdlet', 'Common', 'Get-PSDocument' {
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Cmdlets.Doc.ps1';

    Context 'With -Module' {
        $testModuleSourcePath = Join-Path $here -ChildPath 'TestModule';

        It 'Returns documents' {
            $Null = Import-Module $testModuleSourcePath -Force;
            $result = @(Get-PSDocument -Module 'TestModule');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result.Id | Should -BeIn 'TestModule\TestDocument1', 'TestModule\TestDocument2';
        }

        if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
            $Null = Remove-Module -Name TestModule;
        }

        It 'Loads module with preference' {
            Mock -CommandName 'LoadModule' -ModuleName 'PSDocs';
            $currentLoadingPreference = Get-Variable -Name PSModuleAutoLoadingPreference -ErrorAction SilentlyContinue -ValueOnly;

            try {
                # Test negative case
                $Global:PSModuleAutoLoadingPreference = [System.Management.Automation.PSModuleAutoLoadingPreference]::None;
                $Null = Get-PSDocument -Module 'TestModule';
                Assert-MockCalled -CommandName 'LoadModule' -ModuleName 'PSDocs' -Times 0 -Scope 'It';

                # Test positive case
                $Global:PSModuleAutoLoadingPreference = [System.Management.Automation.PSModuleAutoLoadingPreference]::All;
                $Null = Get-PSDocument -Module 'TestModule';
                Assert-MockCalled -CommandName 'LoadModule' -ModuleName 'PSDocs' -Times 1 -Scope 'It';
            }
            finally {
                if ($Null -eq $currentLoadingPreference) {
                    Remove-Variable -Name PSModuleAutoLoadingPreference -Force -ErrorAction SilentlyContinue;
                }
                else {
                    $Global:PSModuleAutoLoadingPreference = $currentLoadingPreference;
                }
            }
        }

        It 'Use modules already loaded' {
            Mock -CommandName 'GetAutoloadPreference' -ModuleName 'PSDocs' -MockWith {
                return [System.Management.Automation.PSModuleAutoLoadingPreference]::All;
            }
            Mock -CommandName 'LoadModule' -ModuleName 'PSDocs';
            $Null = Import-Module $testModuleSourcePath -Force;
            $result = @(Get-PSDocument -Module 'TestModule')
            Assert-MockCalled -CommandName 'LoadModule' -ModuleName 'PSDocs' -Times 0 -Scope 'It';
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result.Id | Should -BeIn 'TestModule\TestDocument1', 'TestModule\TestDocument2';
        }

        if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
            $Null = Remove-Module -Name TestModule;
        }

        It 'Handles path spaces' {
            # Copy file
            $testParentPath = Join-Path -Path $outputPath -ChildPath 'Program Files\';
            $testDestinationPath = Join-Path -Path $testParentPath -ChildPath 'FromFile.Doc.ps1';
            if (!(Test-Path -Path $testParentPath)) {
                $Null = New-Item -Path $testParentPath -ItemType Directory -Force;
            }
            $Null = Copy-Item -Path $docFilePath -Destination $testDestinationPath -Force;

            $result = @(Get-PSDocument -Path $testDestinationPath);
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -BeGreaterOrEqual 6;

            # Copy module to test path
            $testModuleDestinationPath = Join-Path -Path $testParentPath -ChildPath 'TestModule';
            $Null = Copy-Item -Path $testModuleSourcePath -Destination $testModuleDestinationPath -Recurse -Force;

            # Test modules with spaces in paths
            $Null = Import-Module $testModuleDestinationPath -Force;
            $result = @(Get-PSDocument -Module 'TestModule');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 2;
            $result[0].Id | Should -Be 'TestModule\TestDocument1';
        }

        if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
            $Null = Remove-Module -Name TestModule;
        }

        It 'Returns module and path documents' {
            $Null = Import-Module $testModuleSourcePath -Force;
            $result = @(Get-PSDocument -Path $testModuleSourcePath -Module 'TestModule');
            $result | Should -Not -BeNullOrEmpty;
            $result.Length | Should -Be 4;
            $result.Id | Should -BeIn 'TestModule\TestDocument1', 'TestModule\TestDocument2', '.\TestDocument1', '.\TestDocument2';
        }

        if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
            $Null = Remove-Module -Name TestModule;
        }

        if ($Null -ne (Get-Module -Name TestModule -ErrorAction SilentlyContinue)) {
            $Null = Remove-Module -Name TestModule;
        }
    }
}

Describe 'Get-PSDocumentHeader' -Tag 'Cmdlet', 'Common', 'Get-PSDocumentHeader' {
    $docFilePath = Join-Path -Path $here -ChildPath 'FromFile.Cmdlets.Doc.ps1';

    Context 'With -Path' {
        $testObject = [PSCustomObject]@{
            Name = 'TestObject'
        }
        $invokeParams = @{
            Path = $docFilePath
            InputObject = $testObject
            OutputPath = $outputPath
        }
        It 'Get Metadata header' {
            $result = Invoke-PSDocument @invokeParams -Name 'WithMetadata';
            $result = Get-PSDocumentHeader -Path $outputPath;
            $result | Should -Not -BeNullOrEmpty;
        }
    }
}
