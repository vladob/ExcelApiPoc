. (Join-Path $PSScriptRoot "Deployment.Common.ps1")

Assert-ExcelClosed

$addInRoot = Join-Path (Get-ProductRoot) "AddIn"
$statePath = Join-Path $addInRoot "installation.json"
if (-not (Test-Path -LiteralPath $statePath)) {
    throw "Installation state was not found. Rollback is not available."
}

$state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
$previousXllPath = [string]$state.PreviousXllPath
if ([string]::IsNullOrWhiteSpace($previousXllPath) -or
    -not (Test-Path -LiteralPath $previousXllPath)) {
    throw "A previous installed version is not available for rollback."
}

$currentXllPath = [string]$state.CurrentXllPath
$registrationName = Set-ExcelAddInRegistration -XllPath $previousXllPath
$state.CurrentXllPath = $previousXllPath
$state.PreviousXllPath = $currentXllPath
$state.ExcelRegistrationName = $registrationName
$state.Version = Split-Path -Leaf (Split-Path -Parent $previousXllPath)
$state.InstalledAtUtc = [DateTime]::UtcNow.ToString("o")
$state | ConvertTo-Json | Set-Content -LiteralPath $statePath -Encoding UTF8

Write-Host ""
Write-Host "ExcelApiPoc.AddIn was rolled back successfully." -ForegroundColor Green
Write-Host "Active add-in: $previousXllPath"

