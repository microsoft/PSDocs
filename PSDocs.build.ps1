
param (
    [Parameter(Mandatory = $False)]
    [String]$ModuleVersion,

    [Parameter(Mandatory = $False)]
    [String]$Configuration = 'Debug',

    [Parameter(Mandatory = $False)]
    [String]$ApiKey,

    [Parameter(Mandatory = $False)]
    [Switch]$CodeCoverage = $False,

    [Parameter(Mandatory = $False)]
    [String]$OutputPath = (Join-Path -Path $PWD -ChildPath out/modules)
)

# Copy the PowerShell modules files to the destination path
function CopyModuleFiles {

    param (
        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $True)]
        [String]$DestinationPath
    )

    process {
        $sourcePath = Resolve-Path -Path $Path;

        Get-ChildItem -Path $sourcePath -Recurse -File -Include *.ps1,*.psm1,*.psd1 | Where-Object -FilterScript {
            ($_.FullName -notmatch '(\.(cs|csproj)|(\\|\/)(obj|bin))')
        } | ForEach-Object -Process {
            $filePath = $_.FullName.Replace($sourcePath, $destinationPath);

            $parentPath = Split-Path -Path $filePath -Parent;

            if (!(Test-Path -Path $parentPath)) {
                $Null = New-Item -Path $parentPath -ItemType Directory -Force;
            }

            Copy-Item -Path $_.FullName -Destination $filePath -Force;
        };
    }
}

function SendAppveyorTestResult {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Uri,

        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $False)]
        [String]$Include = '*'
    )

    begin {
        Write-Verbose -Message "[SendAppveyorTestResult] BEGIN::";
    }

    process {

        try {
            $webClient = New-Object -TypeName 'System.Net.WebClient';

            foreach ($resultFile in (Get-ChildItem -Path $Path -Filter $Include -File -Recurse)) {

                Write-Verbose -Message "[SendAppveyorTestResult] -- Uploading file: $($resultFile.FullName)";

                $webClient.UploadFile($Uri, "$($resultFile.FullName)");
            }
        }
        catch {
            throw $_.Exception;
        }
        finally {
            $webClient = $Null;
        }
    }

    end {
        Write-Verbose -Message "[SendAppveyorTestResult] END::";
    }
}

task BuildDotNet {
    exec {
        # Build library
        dotnet publish src/PSDocs -c $Configuration -f net452 -o $(Join-Path -Path $PWD -ChildPath out/modules/PSDocs/desktop)
        dotnet publish src/PSDocs -c $Configuration -f netstandard2.0 -o $(Join-Path -Path $PWD -ChildPath out/modules/PSDocs/core)
    }
}

task CopyModule {
    CopyModuleFiles -Path src/PSDocs -DestinationPath out/modules/PSDocs;
    CopyModuleFiles -Path src/PSDocs.Dsc -DestinationPath out/modules/PSDocs.Dsc;

    # Copy third party notices
    Copy-Item -Path ThirdPartyNotices.txt -Destination out/modules/PSDocs;
}

task UpdateManifest {

    # Update module version
    if (![String]::IsNullOrEmpty($ModuleVersion)) {
        Update-ModuleManifest -Path out/modules/PSDocs/PSDocs.psd1 -ModuleVersion $ModuleVersion;

        Import-Module ./out/modules/PSDocs -Force;

        Update-ModuleManifest -Path out/modules/PSDocs.Dsc/PSDocs.Dsc.psd1 -ModuleVersion $ModuleVersion -RequiredModules @(
            [PSObject]@{ ModuleName = 'PSDocs'; ModuleVersion = "$ModuleVersion" }
        );
    }
}

# Synopsis: Build modules
task BuildModule BuildDotNet, CopyModule, UpdateManifest

# Synopsis: Build help
task BuildHelp BuildModule, PlatyPS, {

    # Generate MAML and about topics
    $Null = New-ExternalHelp -OutputPath out/docs/PSDocs -Path '.\docs\commands\PSDocs\en-US','.\docs\keywords\PSDocs\en-US','.\docs\concepts\PSDocs\en-US' -Force;
    $Null = New-ExternalHelp -OutputPath out/docs/PSDocs.Dsc -Path '.\docs\commands\PSDocs.Dsc\en-US' -Force;

    # Copy generated help into module out path
    $Null = Copy-Item -Path out/docs/PSDocs/* -Destination out/modules/PSDocs/en-US;
    $Null = Copy-Item -Path out/docs/PSDocs/* -Destination out/modules/PSDocs/en-AU;
    $Null = Copy-Item -Path out/docs/PSDocs.Dsc/* -Destination out/modules/PSDocs.Dsc/en-US;
    $Null = Copy-Item -Path out/docs/PSDocs.Dsc/* -Destination out/modules/PSDocs.Dsc/en-AU;
}

task ScaffoldHelp BuildModule, {

    Import-Module (Join-Path -Path $PWD -ChildPath out/modules/PSDocs) -Force;
    Import-Module (Join-Path -Path $PWD -ChildPath out/modules/PSDocs.Dsc) -Force;

    Update-MarkdownHelp -Path '.\docs\commands\PSDocs\en-US';
    Update-MarkdownHelp -Path '.\docs\commands\PSDocs.Dsc\en-US';
}

# Synopsis: Remove temp files.
task Clean {
    Remove-Item -Path out,reports -Recurse -Force -ErrorAction SilentlyContinue;
}

task PublishModule {

    if (![String]::IsNullOrEmpty($ApiKey)) {

        Publish-Module -Path out/modules/PSDocs -NuGetApiKey $ApiKey -Repository PSGallery;
        Publish-Module -Path out/modules/PSDocs.Dsc -NuGetApiKey $ApiKey -Repository PSGallery;
    }
}

task NuGet {
    $Null = Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
}

task Pester {

    # Install pester if v4+ is not currently installed
    if ($Null -eq (Get-Module -Name Pester -ListAvailable | Where-Object -FilterScript { $_.Version -like '4.*' })) {
        Install-Module -Name Pester -MinimumVersion '4.0.0' -Force -Scope CurrentUser -SkipPublisherCheck;
    }

    Import-Module -Name Pester -Verbose:$False;
}

task platyPS {

    # Install pester if v4+ is not currently installed
    if ($Null -eq (Get-Module -Name PlatyPS -ListAvailable)) {
        Install-Module -Name PlatyPS -Force -Scope CurrentUser;
    }

    Import-Module -Name PlatyPS -Verbose:$False;
}

task TestModule Pester, {

    # Run Pester tests
    $pesterParams = @{ Path = $PWD; OutputFile = 'reports/Pester.xml'; OutputFormat = 'NUnitXml'; PesterOption = @{ IncludeVSCodeMarker = $True }; PassThru = $True; };

    if ($CodeCoverage) {
        $pesterParams.Add('CodeCoverage', (Join-Path -Path $PWD -ChildPath 'out/modules/**/*.psm1'));
    }

    if (!(Test-Path -Path reports)) {
        $Null = New-Item -Path reports -ItemType Directory -Force;
    }

    $results = Invoke-Pester @pesterParams;

    if (![String]::IsNullOrEmpty($Env:APPVEYOR_JOB_ID)) {
        SendAppveyorTestResult -Uri "https://ci.appveyor.com/api/testresults/nunit/$($env:APPVEYOR_JOB_ID)" -Path '.\reports' -Include '*.xml';
    }

    # Throw an error if pester tests failed

    if ($Null -eq $results) {
        throw 'Failed to get Pester test results.';
    }
    elseif ($results.FailedCount -gt 0) {
        throw "$($results.FailedCount) tests failed.";
    }
}

# Synopsis: Build and clean.
task . Build, Test

# Synopsis: Build the project
task Build Clean, BuildModule, BuildHelp

task Test Build, TestModule

task Publish UpdateManifest, PublishModule
