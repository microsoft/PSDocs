
param (
    [Parameter(Mandatory = $False)]
    [String]$ModuleVersion,

    [Parameter(Mandatory = $False)]
    [AllowNull()]
    [String]$ReleaseVersion,

    [Parameter(Mandatory = $False)]
    [String]$Configuration = 'Debug',

    [Parameter(Mandatory = $False)]
    [String]$NuGetApiKey,

    [Parameter(Mandatory = $False)]
    [Switch]$CodeCoverage = $False,

    [Parameter(Mandatory = $False)]
    [Switch]$Benchmark = $False,

    [Parameter(Mandatory = $False)]
    [String]$ArtifactPath = (Join-Path -Path $PWD -ChildPath out/modules)
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

        Get-ChildItem -Path $sourcePath -Recurse -File -Include *.ps1,*.psm1,*.psd1,*.ps1xml | Where-Object -FilterScript {
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

# Synopsis: Build modules only
task BuildModule BuildDotNet, CopyModule, VersionModule

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

task VersionModule {
    if (![String]::IsNullOrEmpty($ReleaseVersion)) {
        Write-Verbose -Message "[VersionModule] -- ReleaseVersion: $ReleaseVersion";
        $ModuleVersion = $ReleaseVersion;
    }

    if (![String]::IsNullOrEmpty($ModuleVersion)) {
        Write-Verbose -Message "[VersionModule] -- ModuleVersion: $ModuleVersion";

        $version = $ModuleVersion;
        $revision = [String]::Empty;

        if ($version -like '*-*') {
            [String[]]$versionParts = $version.Split('-', [System.StringSplitOptions]::RemoveEmptyEntries);
            $version = $versionParts[0];

            if ($versionParts.Length -eq 2) {
                $revision = $versionParts[1];
            }
        }

        Write-Verbose -Message "[VersionModule] -- Using Version: $version";
        Write-Verbose -Message "[VersionModule] -- Using Revision: $revision";

        # Update module version
        if (![String]::IsNullOrEmpty($version)) {
            Write-Verbose -Message "[VersionModule] -- Updating module manifest ModuleVersion for PSDocs";
            Update-ModuleManifest -Path (Join-Path -Path $ArtifactPath -ChildPath PSDocs/PSDocs.psd1) -ModuleVersion $version;

            # Updating PSModulePath is required for Update-ModuleManifest with -RequiredModules to work
            $Env:PSModulePath += ";$ArtifactPath";

            Write-Verbose -Message "[VersionModule] -- Updating module manifest ModuleVersion for PSDocs.Dsc";
            Import-Module (Join-Path -Path $ArtifactPath -ChildPath PSDocs) -Force;
            $requiredVersion = @(New-Object -TypeName Microsoft.PowerShell.Commands.ModuleSpecification -ArgumentList @{ ModuleName = 'PSDocs'; ModuleVersion = "$version"; });
            Update-ModuleManifest -Path (Join-Path -Path $ArtifactPath -ChildPath PSDocs.Dsc/PSDocs.Dsc.psd1) -ModuleVersion $version -RequiredModules $requiredVersion -Verbose;
        }

        # Update pre-release version
        if (![String]::IsNullOrEmpty($revision)) {
            Write-Verbose -Message "[VersionModule] -- Updating module manifest Prerelease for PSDocs";
            Update-ModuleManifest -Path (Join-Path -Path $ArtifactPath -ChildPath PSDocs/PSDocs.psd1) -Prerelease $revision;

            Write-Verbose -Message "[VersionModule] -- Updating module manifest Prerelease for PSDocs.Dsc";
            Import-Module (Join-Path -Path $ArtifactPath -ChildPath PSDocs) -Force;
            Update-ModuleManifest -Path (Join-Path -Path $ArtifactPath -ChildPath PSDocs.Dsc/PSDocs.Dsc.psd1) -Prerelease $revision;
        }
    }
}

task ReleaseModule VersionModule, {
    if (![String]::IsNullOrEmpty($NuGetApiKey)) {
        # Publish to PowerShell Gallery
        Publish-Module -Path (Join-Path -Path $ArtifactPath -ChildPath PSDocs) -NuGetApiKey $NuGetApiKey;
        Publish-Module -Path (Join-Path -Path $ArtifactPath -ChildPath PSDocs.Dsc) -NuGetApiKey $NuGetApiKey;
    }
}

task NuGet {
    $Null = Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
}

# Synopsis: Get Pester
task Pester {
    # Install Pester if v4+ is not currently installed
    if ($Null -eq (Get-Module -Name Pester -ListAvailable | Where-Object -FilterScript { $_.Version -like '4.*' })) {
        Install-Module -Name Pester -MinimumVersion '4.0.0' -Force -Scope CurrentUser -SkipPublisherCheck;
    }
    Import-Module -Name Pester -Verbose:$False;
}

# Synopsis: Get PlatyPS
task platyPS {
    # Install PlatyPS if not currently installed
    if ($Null -eq (Get-Module -Name PlatyPS -ListAvailable)) {
        Install-Module -Name PlatyPS -Force -Scope CurrentUser;
    }
    Import-Module -Name PlatyPS -Verbose:$False;
}

# Synopsis: Get PSScriptAnalyzer
task PSScriptAnalyzer {
    # Install PSScriptAnalyzer if not currently installed
    if ($Null -eq (Get-Module -Name PSScriptAnalyzer -ListAvailable)) {
        Install-Module -Name PSScriptAnalyzer -Force -Scope CurrentUser;
    }
    Import-Module -Name PSScriptAnalyzer -Verbose:$False;
}

# Synopsis: Run Pester tests
task TestModule Pester, PSScriptAnalyzer, {
    $pesterParams = @{ Path = $PWD; OutputFile = 'reports/Pester.xml'; OutputFormat = 'NUnitXml'; PesterOption = @{ IncludeVSCodeMarker = $True }; PassThru = $True; };

    if ($CodeCoverage) {
        $pesterParams.Add('CodeCoverage', (Join-Path -Path $PWD -ChildPath 'out/modules/**/*.psm1'));
    }

    if (!(Test-Path -Path reports)) {
        $Null = New-Item -Path reports -ItemType Directory -Force;
    }

    $results = Invoke-Pester @pesterParams;

    # Throw an error if pester tests failed
    if ($Null -eq $results) {
        throw 'Failed to get Pester test results.';
    }
    elseif ($results.FailedCount -gt 0) {
        throw "$($results.FailedCount) tests failed.";
    }
}

# Synopsis: Run script analyzer
task Analyze Build, PSScriptAnalyzer, {

    Invoke-ScriptAnalyzer -Path out/modules/PSDocs;
    Invoke-ScriptAnalyzer -Path out/modules/PSDocs.Dsc;
}

# Synopsis: Build and clean.
task . Build, Test

# Synopsis: Build the project
task Build Clean, BuildModule, BuildHelp, VersionModule

task Test Build, TestModule

task Release ReleaseModule
