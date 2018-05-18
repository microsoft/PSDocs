#
# Common helper functions
#

function CreatePath {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [String]$Path,

        [Switch]$Clean = $False
    )

    process {

        # If the directory does not exist, force the creation of the path
        if (!(Test-Path -Path $Path)) {
            Write-Verbose -Message "[CreatePath] -- Creating path: $Path";

            New-Item -Path $Path -ItemType Directory -Force | Out-Null;
        } else {
            Write-Verbose -Message "[CreatePath] -- Path already exists: $Path";

            if ($Clean) {
                 Write-Verbose -Message "[CreatePath] -- Cleaning path: $Path";

                 Remove-Item -Path "$Path\" -Force -Recurse -Confirm:$False;

                 New-Item -Path $Path -ItemType Directory -Force | Out-Null;
            }
        }
    }
}

function RunTest {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $True)]
        [String]$SourcePath,

        [Parameter(Mandatory = $True)]
        [String]$OutputPath,

        [Parameter(Mandatory = $False)]
        [Switch]$CodeCoverage = $False
    )

    begin {
        Write-Verbose -Message "[RunTest] BEGIN::";
    }

    process {

        Write-Verbose -Message "[RunTest] -- Running tests: $Path";

        # Run Pester tests
        $pesterParams = @{ Path = $Path; OutputFile = "$OutputPath\Pester.xml"; OutputFormat = 'NUnitXml'; PesterOption = @{ IncludeVSCodeMarker = $True }; PassThru = $True; };

        if ($CodeCoverage) {
            $pesterParams.Add('CodeCoverage', "$SourcePath\**\*.psm1");
        }

        Invoke-Pester @pesterParams;
    }

    end {
        Write-Verbose -Message "[RunTest] END::";
    }
}

function BuildModule {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [String]$Module,

        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $True)]
        [String]$OutputPath
    )

    begin {
        Write-Verbose -Message "[BuildModule] BEGIN::";
    }

    process {

        if (Test-Path -Path ("$OutputPath\$Module")) {
            Remove-Item -Path "$OutputPath\$Module" -Recurse -Force;
        }

        $sourcePath = Join-Path -Path $Path -ChildPath $Module;
        $destinationPath = Join-Path -Path $OutputPath -ChildPath $Module;

        if ($Null -ne (Get-ChildItem -Path $sourcePath -Filter '*.csproj')) {

            Write-Verbose -Message "[BuildModule] -- Building .NET modules";

            # Restore packages
            DotNetRestore -Path $sourcePath;
            
            # Build and publish
            DotNetPublish -Path $sourcePath;
        }

        Write-Verbose -Message "[BuildModule] -- Copying output to: $destinationPath";

        Get-ChildItem -Path $sourcePath -Recurse | Where-Object -FilterScript {
            ($_.FullName -notmatch '(\.(cs|csproj)|(\\|\/)obj)')
        } | ForEach-Object -Process {
            $filePath = $_.FullName.Replace($sourcePath, $destinationPath);

            Copy-Item -Path $_.FullName -Destination $filePath -Force;
        };

        # Copy-Item -Path $sourcePath -Destination $OutputPath -Recurse -Force;

    }

    end {
        Write-Verbose -Message "[BuildModule] END::";
    }
}

function DotNetRestore {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {

        Write-Verbose -Message "[DotNetRestore] -- Restoring .NET dependencies to: $Path";

        dotnet restore $Path;
    }
}

function DotNetPublish {
    
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {

        $projectFiles = Get-ChildItem -Path $Path -Filter '*.csproj';

        foreach ($p in $projectFiles) {
            $frameworks = GetProjectFramework -Path $p.FullName -Verbose:$VerbosePreference;

            foreach ($f in $frameworks) {
                dotnet publish -f $f $p.FullName;
            }
        }
    }
}

function GetProjectFramework {

    [CmdletBinding()]
    [OutputType([String])]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )

    process {
        Write-Verbose -Message "[GetProjectFramework] -- Checking .NET framework support for: $Path";

        $csProject = [Xml](Get-Content -Path $Path);

        $frameworks = $csProject.Project.PropertyGroup.TargetFrameworks;

        foreach ($f in $frameworks) {
            $f -Split ';';
        }
    }
}

function PackageModule {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [String]$Module,

        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $True)]
        [String]$OutputPath
    )

    begin {
        Write-Verbose -Message "[PackageModule] BEGIN::";
    }

    process {

        Write-Verbose -Message "[PackageModule] -- Packaging module: $Module";

        $targetFile = "$OutputPath\$Module.zip";

        Compress-Archive -DestinationPath $targetFile -Path "$Path\$Module" -Force;

        Write-Verbose -Message "[PackageModule] -- Saved module to: $targetFile";
    }

    end {
        Write-Verbose -Message "[PackageModule] END::";
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