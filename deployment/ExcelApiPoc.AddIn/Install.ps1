param(
    [string]$Version = "1.0.0",
    [string]$ApiBaseUrl = "http://10.0.0.249:5080"
)

. (Join-Path $PSScriptRoot "Deployment.Common.ps1")

Assert-ExcelClosed
Assert-64BitMicrosoft365

if ($Version -notmatch '^\d+\.\d+\.\d+([-.][A-Za-z0-9.]+)?$') {
    throw "Version '$Version' is not a valid deployment version."
}

$apiUri = $null
if (-not [Uri]::TryCreate($ApiBaseUrl, [UriKind]::Absolute, [ref]$apiUri) -or
    ($apiUri.Scheme -ne "http" -and $apiUri.Scheme -ne "https")) {
    throw "ApiBaseUrl must be a valid HTTP or HTTPS address."
}

$sourceXll = Join-Path $PSScriptRoot "payload\ExcelApiPoc.AddIn.xll"
if (-not (Test-Path -LiteralPath $sourceXll)) {
    throw "The deployment payload is incomplete: '$sourceXll' was not found."
}

$productRoot = Get-ProductRoot
$addInRoot = Join-Path $productRoot "AddIn"
$versionRoot = Join-Path $addInRoot $Version
$targetXll = Join-Path $versionRoot "ExcelApiPoc.AddIn.xll"
$statePath = Join-Path $addInRoot "installation.json"
$settingsPath = Join-Path $productRoot "settings.json"

$previousXllPath = $null
if (Test-Path -LiteralPath $statePath) {
    $previousState = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
    if ($null -ne $previousState.CurrentXllPath -and
        ([string]$previousState.CurrentXllPath -ne $targetXll) -and
        (Test-Path -LiteralPath ([string]$previousState.CurrentXllPath))) {
        $previousXllPath = [string]$previousState.CurrentXllPath
    }
}

New-Item -ItemType Directory -Path $versionRoot -Force | Out-Null
Copy-Item -LiteralPath $sourceXll -Destination $targetXll -Force
Unblock-File -LiteralPath $targetXll

$registrationName = Set-ExcelAddInRegistration -XllPath $targetXll

if (-not (Test-Path -LiteralPath $settingsPath)) {
    New-Item -ItemType Directory -Path $productRoot -Force | Out-Null
    [ordered]@{ ApiBaseUrl = $ApiBaseUrl.TrimEnd('/') } |
        ConvertTo-Json |
        Set-Content -LiteralPath $settingsPath -Encoding UTF8
}

[ordered]@{
    Product = "ExcelApiPoc.AddIn"
    Version = $Version
    Architecture = "x64"
    CurrentXllPath = $targetXll
    PreviousXllPath = $previousXllPath
    ExcelRegistrationName = $registrationName
    InstalledAtUtc = [DateTime]::UtcNow.ToString("o")
} |
    ConvertTo-Json |
    Set-Content -LiteralPath $statePath -Encoding UTF8

Write-Host ""
Write-Host "ExcelApiPoc.AddIn $Version was installed successfully." -ForegroundColor Green
Write-Host "Add-in: $targetXll"
Write-Host "API:    $ApiBaseUrl"
Write-Host "Start Excel and use Settings > Test connection to verify the installation."
