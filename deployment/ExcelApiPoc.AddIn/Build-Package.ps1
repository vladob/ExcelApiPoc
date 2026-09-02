param(
    [string]$Version = "1.0.0",
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "artifacts")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($Version -notmatch '^\d+\.\d+\.\d+([-.][A-Za-z0-9.]+)?$') {
    throw "Version '$Version' is not a valid deployment version."
}

$projectPath = Join-Path $RepositoryRoot "ExcelApiPoc.AddIn\ExcelApiPoc.AddIn.csproj"
$solutionPath = Join-Path $RepositoryRoot "ExcelApiPoc.sln"
$vswherePath = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path -LiteralPath $vswherePath)) {
    throw "Visual Studio Installer's vswhere.exe was not found."
}

$msbuildPath = & $vswherePath `
    -latest `
    -requires Microsoft.Component.MSBuild `
    -find "MSBuild\**\Bin\MSBuild.exe" |
    Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($msbuildPath)) {
    throw "MSBuild was not found in the installed Visual Studio instance."
}

Write-Host "Restoring NuGet packages..."
& $msbuildPath $solutionPath `
    /t:Restore `
    /p:RestorePackagesConfig=true `
    /verbosity:minimal
if ($LASTEXITCODE -ne 0) {
    throw "NuGet restore failed with exit code $LASTEXITCODE."
}

Write-Host "Building the x64 Release add-in..."
& $msbuildPath $projectPath `
    /t:Rebuild `
    /p:Configuration=Release `
    /p:Platform=AnyCPU `
    /verbosity:minimal
if ($LASTEXITCODE -ne 0) {
    throw "The Release build failed with exit code $LASTEXITCODE."
}

$releaseRoot = Join-Path $RepositoryRoot "ExcelApiPoc.AddIn\bin\Release"
$packedXll = Get-ChildItem -LiteralPath $releaseRoot `
    -Filter "ExcelApiPoc.AddIn-AddIn64-packed.xll" `
    -File `
    -Recurse |
    Select-Object -First 1
if ($null -eq $packedXll) {
    throw "The 64-bit packed XLL was not found beneath '$releaseRoot'."
}

$packageName = "ExcelApiPoc.AddIn-$Version-x64"
$stagingRoot = Join-Path $OutputDirectory $packageName
$payloadRoot = Join-Path $stagingRoot "payload"
$zipPath = Join-Path $OutputDirectory "$packageName.zip"

if (Test-Path -LiteralPath $stagingRoot) {
    Remove-Item -LiteralPath $stagingRoot -Recurse -Force
}
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

New-Item -ItemType Directory -Path $payloadRoot -Force | Out-Null
$packageFiles = @(
    "Deployment.Common.ps1",
    "Install.ps1",
    "Install.cmd",
    "Rollback.ps1",
    "Rollback.cmd",
    "Uninstall.ps1",
    "Uninstall.cmd",
    "README.txt"
)
foreach ($packageFile in $packageFiles) {
    Copy-Item `
        -LiteralPath (Join-Path $PSScriptRoot $packageFile) `
        -Destination (Join-Path $stagingRoot $packageFile)
}

$deployedXll = Join-Path $payloadRoot "ExcelApiPoc.AddIn.xll"
Copy-Item -LiteralPath $packedXll.FullName -Destination $deployedXll
$xllHash = (Get-FileHash -LiteralPath $deployedXll -Algorithm SHA256).Hash

[ordered]@{
    Product = "ExcelApiPoc.AddIn"
    Version = $Version
    Architecture = "x64"
    ApiBaseUrl = "http://10.0.0.249:5080"
    SourceCommit = (& git -C $RepositoryRoot rev-parse HEAD).Trim()
    BuiltAtUtc = [DateTime]::UtcNow.ToString("o")
    XllSha256 = $xllHash
} |
    ConvertTo-Json |
    Set-Content -LiteralPath (Join-Path $stagingRoot "release.json") -Encoding UTF8

Compress-Archive -LiteralPath $stagingRoot -DestinationPath $zipPath -CompressionLevel Optimal

Write-Host ""
Write-Host "Deployment package created successfully." -ForegroundColor Green
Write-Host $zipPath
Write-Host "XLL SHA-256: $xllHash"
