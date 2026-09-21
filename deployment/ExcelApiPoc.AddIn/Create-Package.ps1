param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+([-.][A-Za-z0-9.]+)?$')]
    [string]$Version,

    [switch]$SkipBuild,
    [switch]$AllowDirty
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ------------------------------------------------------------
# Base paths
# ------------------------------------------------------------

$DeploymentRoot = $PSScriptRoot
$RepoRoot = (Resolve-Path (Join-Path $DeploymentRoot "..\..")).Path
$ProjectFile = Join-Path $RepoRoot "ExcelApiPoc.AddIn\ExcelApiPoc.AddIn.csproj"

$BuildXll = Join-Path $RepoRoot "ExcelApiPoc.AddIn\bin\Release\ExcelApiPoc.AddIn-AddIn64-packed.xll"

$ArtifactsRoot = Join-Path $DeploymentRoot "artifacts"
$PackageName   = "ExcelApiPoc.AddIn-$Version-x64"
$PackageFolder = Join-Path $ArtifactsRoot $PackageName
$PayloadFolder = Join-Path $PackageFolder "payload"
$PayloadXll    = Join-Path $PayloadFolder "ExcelApiPoc.AddIn.xll"
$ZipFile       = Join-Path $ArtifactsRoot "$PackageName.zip"

# ------------------------------------------------------------
# Git identity / release safety
# ------------------------------------------------------------

Push-Location $RepoRoot

try {
    $Commit = (git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($Commit)) {
        throw "Unable to determine Git commit."
    }

    $Branch = (git branch --show-current).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to determine Git branch."
    }

    $Tag = (git describe --tags --exact-match HEAD 2>$null)
    if ($LASTEXITCODE -ne 0) {
        $Tag = $null
    }
    elseif ($null -ne $Tag) {
        $Tag = $Tag.Trim()
    }

    $WorkingTreeChanges = @(git status --porcelain)
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to determine Git working-tree status."
    }

    if (-not $AllowDirty -and $WorkingTreeChanges.Count -gt 0) {
        throw @"
The Git working tree is not clean.

Commit, stash, or discard local changes before creating a deployment package.
Use -AllowDirty only for an intentional local test package.
"@
    }
}
finally {
    Pop-Location
}

# ------------------------------------------------------------
# Build Release package
# ------------------------------------------------------------

if (-not $SkipBuild) {
    Write-Host "Building ExcelApiPoc.AddIn in Release mode..."

    & dotnet build $ProjectFile --configuration Release

    if ($LASTEXITCODE -ne 0) {
        throw "Release build failed."
    }
}
else {
    Write-Host "Skipping Release build (-SkipBuild)."
}

# ------------------------------------------------------------
# Validate build output
# ------------------------------------------------------------

if (-not (Test-Path -LiteralPath $BuildXll)) {
    throw @"
Packed x64 XLL not found:

$BuildXll

Build ExcelApiPoc.AddIn in Release mode first, or run this script without -SkipBuild.
"@
}

# ------------------------------------------------------------
# Recreate package directory
# ------------------------------------------------------------

New-Item -ItemType Directory -Force -Path $ArtifactsRoot | Out-Null

if (Test-Path -LiteralPath $PackageFolder) {
    Write-Host "Removing old package folder..."
    Remove-Item -LiteralPath $PackageFolder -Recurse -Force
}

if (Test-Path -LiteralPath $ZipFile) {
    Write-Host "Removing old package ZIP..."
    Remove-Item -LiteralPath $ZipFile -Force
}

New-Item -ItemType Directory -Force -Path $PayloadFolder | Out-Null

# ------------------------------------------------------------
# Copy deployment scripts/files
# ------------------------------------------------------------

$DeploymentFiles = @(
    "Install.cmd",
    "Install.ps1",
    "Rollback.cmd",
    "Rollback.ps1",
    "Uninstall.cmd",
    "Uninstall.ps1",
    "Deployment.Common.ps1",
    "README.txt"
)

foreach ($File in $DeploymentFiles) {
    $Source = Join-Path $DeploymentRoot $File

    if (-not (Test-Path -LiteralPath $Source)) {
        throw "Required deployment file not found: $Source"
    }

    Copy-Item -LiteralPath $Source -Destination $PackageFolder -Force
}

# ------------------------------------------------------------
# Copy packed x64 XLL
# ------------------------------------------------------------

Write-Host "Copying packed x64 Add-in..."
Copy-Item -LiteralPath $BuildXll -Destination $PayloadXll -Force

# ------------------------------------------------------------
# Compute payload metadata
# ------------------------------------------------------------

$PayloadInfo = Get-Item -LiteralPath $PayloadXll
$Sha256 = (Get-FileHash -LiteralPath $PayloadXll -Algorithm SHA256).Hash

# ------------------------------------------------------------
# Create release.json
# ------------------------------------------------------------

$Release = [ordered]@{
    Product        = "ExcelApiPoc.AddIn"
    Version        = $Version
    Architecture   = "x64"
    ApiBaseUrl     = "http://10.0.0.249:5080"
    SourceCommit   = $Commit
    SourceBranch   = $Branch
    SourceTag      = $Tag
    BuiltAtUtc     = [DateTime]::UtcNow.ToString("o")
    XllFileName    = "ExcelApiPoc.AddIn.xll"
    XllSizeBytes   = $PayloadInfo.Length
    XllSha256      = $Sha256
}

$ReleaseJson = Join-Path $PackageFolder "release.json"

$Release |
    ConvertTo-Json -Depth 3 |
    Set-Content -LiteralPath $ReleaseJson -Encoding UTF8

# ------------------------------------------------------------
# Create ZIP
# ------------------------------------------------------------

Write-Host "Creating ZIP..."
Compress-Archive -Path (Join-Path $PackageFolder "*") -DestinationPath $ZipFile -CompressionLevel Optimal

$ZipSha256 = (Get-FileHash -LiteralPath $ZipFile -Algorithm SHA256).Hash

# ------------------------------------------------------------
# Result
# ------------------------------------------------------------

Write-Host ""
Write-Host "========================================="
Write-Host " Deployment package created successfully"
Write-Host "========================================="
Write-Host ""
Write-Host "Version : $Version"
Write-Host "Commit  : $Commit"
Write-Host "Branch  : $Branch"

if (-not [string]::IsNullOrWhiteSpace($Tag)) {
    Write-Host "Tag     : $Tag"
}

Write-Host "XLL SHA : $Sha256"
Write-Host "ZIP SHA : $ZipSha256"
Write-Host ""
Write-Host "Folder:"
Write-Host "  $PackageFolder"
Write-Host ""
Write-Host "ZIP:"
Write-Host "  $ZipFile"
Write-Host ""
