#
# Unit tests for the options handling
#

[CmdletBinding()]
param (

)

# Setup error handling
$ErrorActionPreference = 'Stop';
Set-StrictMode -Version latest;

# Setup tests paths
$rootPath = $PWD;
$here = Split-Path -Parent $MyInvocation.MyCommand.Path;
Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;

Describe 'New-PSDocumentOption' -Tag 'Option' {
    Context 'Read psdocs.yml' {

        try {
            Push-Location -Path $here;
            It 'can read default YAML' {
                $option = New-PSDocumentOption;
                $option.Generator | Should -Be 'PSDocs';
            }
        }
        finally {
            Pop-Location;
        }
    }

    Context 'Read Configuration' {
        It 'from default' {
            $option = New-PSDocumentOption -Default;
            $option.Configuration.Count | Should -Be 0;
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Configuration.Key1' = 'Value1' };
            $option.Configuration.Key1 | Should -Be 'Value1';
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Configuration.Key1 | Should -Be 'Value2';
            $option.Configuration.'UnitTests.String.1' | Should -Be 'Config string 1'
        }
    }

    Context 'Read Execution.LanguageMode' {
        It 'from default' {
            $option = New-PSDocumentOption -Default;
            $option.Execution.LanguageMode | Should -Be 'FullLanguage';
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' };
            $option.Execution.LanguageMode | Should -Be 'ConstrainedLanguage';
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Execution.LanguageMode | Should -Be 'ConstrainedLanguage'
        }
    }

    Context 'Read Markdown.Encoding' {
        It 'from default' {
            $option = New-PSDocumentOption -Default;
            $option.Markdown.Encoding | Should -Be 'Default';
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
            $option = New-PSDocumentOption -Default;
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
            $option = New-PSDocumentOption -Default;
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

    Context 'Read Markdown.ColumnPadding' {
        It 'from default' {
            $option = New-PSDocumentOption -Default;
            $option.Markdown.ColumnPadding | Should -Be 'MatchHeader';
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Markdown.ColumnPadding' = 'Single' };
            $option.Markdown.ColumnPadding | Should -Be 'Single';
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Markdown.ColumnPadding | Should -Be 'Single';
        }
    }

    Context 'Read Markdown.UseEdgePipes' {
        It 'from default' {
            $option = New-PSDocumentOption -Default;
            $option.Markdown.UseEdgePipes | Should -Be 'WhenRequired';
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Markdown.UseEdgePipes' = 'Always' };
            $option.Markdown.UseEdgePipes | Should -Be 'Always';
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Markdown.UseEdgePipes | Should -Be 'Always';
        }
    }

    Context 'Read Output.Culture' {
        It 'from default' {
            $option = New-PSDocumentOption -Default;
            $option.Output.Culture | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Output.Culture' = 'en-ZZ' };
            $option.Output.Culture | Should -Be 'en-ZZ';
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Output.Culture | Should -Be 'en-ZZ';
        }
    }

    Context 'Read Output.Path' {
        It 'from default' {
            $option = New-PSDocumentOption -Default;
            $option.Output.Path | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Output.Path' = $here };
            $option.Output.Path | Should -Be $here;
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Output.Path | Should -Be 'out/';
        }
    }
}
