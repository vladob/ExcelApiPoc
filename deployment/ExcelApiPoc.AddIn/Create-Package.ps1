param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

# ------------------------------------------------------------
# Base paths
# ------------------------------------------------------------

$DeploymentRoot = $PSScriptRoot
$RepoRoot = Resolve-Path (Join-Path $DeploymentRoot "..\..")

$BuildXll = Join-Path $RepoRoot `
    "ExcelApiPoc.AddIn\bin\Release\ExcelApiPoc.AddIn-AddIn64-packed.xll"

$ArtifactsRoot = Join-Path $DeploymentRoot "artifacts"
$PackageName   = "ExcelApiPoc.AddIn-$Version-x64"
$PackageFolder = Join-Path $ArtifactsRoot $PackageName
$PayloadFolder = Join-Path $PackageFolder "payload"
$PayloadXll    = Join-Path $PayloadFolder "ExcelApiPoc.AddIn.xll"
$ZipFile       = Join-Path $ArtifactsRoot "$PackageName.zip"

# ------------------------------------------------------------
# Validate build output
# ------------------------------------------------------------

if (-not (Test-Path $BuildXll)) {
    throw @"
Packed x64 XLL not found:

$BuildXll

Build ExcelApiPoc.AddIn in Release mode first.
"@
}

# ------------------------------------------------------------
# Recreate package directory
# ------------------------------------------------------------

if (Test-Path $PackageFolder) {
    Write-Host "Removing old package folder..."
    Remove-Item $PackageFolder -Recurse -Force
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

    if (-not (Test-Path $Source)) {
        throw "Required deployment file not found: $Source"
    }

    Copy-Item $Source $PackageFolder -Force
}

# ------------------------------------------------------------
# Copy packed x64 XLL
# ------------------------------------------------------------

Write-Host "Copying packed x64 Add-in..."

Copy-Item `
    $BuildXll `
    $PayloadXll `
    -Force

# ------------------------------------------------------------
# Obtain Git commit
# ------------------------------------------------------------

Push-Location $RepoRoot

try {
    $Commit = (git rev-parse HEAD).Trim()

    if ($LASTEXITCODE -ne 0) {
        throw "Unable to determine Git commit."
    }
}
finally {
    Pop-Location
}

# ------------------------------------------------------------
# Compute SHA256
# ------------------------------------------------------------

$Sha256 = (Get-FileHash `
    $PayloadXll `
    -Algorithm SHA256).Hash

# ------------------------------------------------------------
# Create release.json
# ------------------------------------------------------------

$Release = [ordered]@{
    Product      = "ExcelApiPoc.AddIn"
    Version      = $Version
    Architecture = "x64"
    ApiBaseUrl   = "http://10.0.0.249:5080"
    SourceCommit = $Commit
    BuiltAtUtc   = [DateTime]::UtcNow.ToString("o")
    XllSha256    = $Sha256
}

$ReleaseJson = Join-Path $PackageFolder "release.json"

$Release |
    ConvertTo-Json |
    Set-Content `
        -Path $ReleaseJson `
        -Encoding UTF8

# ------------------------------------------------------------
# Create ZIP
# ------------------------------------------------------------

if (Test-Path $ZipFile) {
    Remove-Item $ZipFile -Force
}

Write-Host "Creating ZIP..."

Compress-Archive `
    -Path "$PackageFolder\*" `
    -DestinationPath $ZipFile `
    -CompressionLevel Optimal

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
Write-Host "SHA256  : $Sha256"
Write-Host ""
Write-Host "Folder:"
Write-Host "  $PackageFolder"
Write-Host ""
Write-Host "ZIP:"
Write-Host "  $ZipFile"
Write-Host ""