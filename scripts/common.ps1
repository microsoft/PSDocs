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
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [String]$TestGroup,

        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $True)]
        [String]$OutputPath
    )

    begin {
        Write-Verbose -Message "[RunTest] BEGIN::";
    }

    process {

        $currentPath = $PWD;

        try {
            Set-Location -Path "$Path\$TestGroup.Tests" -ErrorAction Stop;

            Write-Verbose -Message "[RunTest] -- Running tests: $Path\$TestGroup.Tests";

            # Run Pester tests
            $pesterParams = @{ OutputFile = "$OutputPath\$TestGroup.xml"; OutputFormat = 'NUnitXml'; PesterOption = @{ IncludeVSCodeMarker = $True }; };

            Invoke-Pester @pesterParams;

        } finally {
            Set-Location -Path $currentPath;
        }
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

        Copy-Item -Path "$Path\$Module" -Destination $OutputPath -Recurse -Force;
    }

    end {
        Write-Verbose -Message "[BuildModule] END::";
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