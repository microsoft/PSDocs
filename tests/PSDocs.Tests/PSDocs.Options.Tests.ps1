# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for the options handling
#

[CmdletBinding()]
param ()

BeforeAll {
    # Setup error handling
    $ErrorActionPreference = 'Stop';
    Set-StrictMode -Version latest;

    # Setup tests paths
    $rootPath = $PWD;
    $here = Split-Path -Parent $MyInvocation.MyCommand.Path;
    Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSDocs) -Force;
    $emptyOptionsFilePath = Join-Path -Path $here -ChildPath 'psdocs.yml';
}
Describe 'New-PSDocumentOption' -Tag 'Option' {
    Context 'Read psdocs.yml' {
        BeforeAll {
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
    }

    Context 'Read Configuration' {
        It 'from default' {
            $option = New-PSDocumentOption -Default;
            $option.Configuration.Count | Should -Be 0;
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Configuration.Key1' = 'Value1'; 'Configuration.BoolValue' = $True };
            $option.Configuration.Key1 | Should -Be 'Value1';
            $option.Configuration.BoolValue | Should -Be $True;
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Configuration.Key1 | Should -Be 'Value2';
            $option.Configuration.'UnitTests.String.1' | Should -Be 'Config string 1';
            $option.Configuration.'UnitTests.Bool.1' | Should -Be $True;
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

    Context 'Read Input.Format' {
        It 'from default' {
            $option = New-PSDocumentOption -Default;
            $option.Input.Format | Should -Be 'Detect';
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Input.Format' = 'Yaml' };
            $option.Input.Format | Should -Be 'Yaml';
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Input.Format | Should -Be 'Yaml'
        }

        It 'from Environment' {
            try {
                $Env:PSDOCS_INPUT_FORMAT = 'Yaml';
                $option = New-PSDocumentOption;
                $option.Input.Format | Should -Be 'Yaml';
            }
            finally {
                Remove-Item 'Env:PSDOCS_INPUT_FORMAT' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSDocumentOption -Format 'Yaml' -Path $emptyOptionsFilePath;
            $option.Input.Format | Should -Be 'Yaml';
        }
    }

    Context 'Read Input.ObjectPath' {
        It 'from default' {
            $option = New-PSDocumentOption -Default;
            $option.Input.ObjectPath | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Input.ObjectPath' = 'items' };
            $option.Input.ObjectPath | Should -Be 'items';
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Input.ObjectPath | Should -Be 'items'
        }

        It 'from Environment' {
            try {
                $Env:PSDOCS_INPUT_OBJECTPATH = 'items';
                $option = New-PSDocumentOption;
                $option.Input.ObjectPath | Should -Be 'items';
            }
            finally {
                Remove-Item 'Env:PSDOCS_INPUT_OBJECTPATH' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSDocumentOption -InputObjectPath 'items' -Path $emptyOptionsFilePath;
            $option.Input.ObjectPath | Should -Be 'items';
        }
    }

    Context 'Read Input.PathIgnore' {
        It 'from default' {
            $option = New-PSDocumentOption -Default;
            $option.Input.PathIgnore | Should -Be $Null;
        }

        It 'from Hashtable' {
            $option = New-PSDocumentOption -Option @{ 'Input.PathIgnore' = 'ignore.cs' };
            $option.Input.PathIgnore | Should -Be 'ignore.cs';
        }

        It 'from YAML' {
            $option = New-PSDocumentOption -Option (Join-Path -Path $here -ChildPath 'PSDocs.Tests.yml');
            $option.Input.PathIgnore | Should -Be '*.Designer.cs';
        }

        It 'from Environment' {
            try {
                # With single item
                $Env:PSDOCS_INPUT_PATHIGNORE = 'ignore.cs';
                $option = New-PSDocumentOption;
                $option.Input.PathIgnore | Should -Be 'ignore.cs';

                # With array
                $Env:PSDOCS_INPUT_PATHIGNORE = 'ignore.cs;*.Designer.cs';
                $option = New-PSDocumentOption;
                $option.Input.PathIgnore | Should -Be 'ignore.cs', '*.Designer.cs';
            }
            finally {
                Remove-Item 'Env:PSDOCS_INPUT_PATHIGNORE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSDocumentOption -InputPathIgnore 'ignore.cs' -Path $emptyOptionsFilePath;
            $option.Input.PathIgnore | Should -Be 'ignore.cs';
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
