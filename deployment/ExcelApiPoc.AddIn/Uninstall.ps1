param(
    [switch]$RemoveUserData
)

. (Join-Path $PSScriptRoot "Deployment.Common.ps1")

Assert-ExcelClosed
Remove-ExcelAddInRegistration

$productRoot = Get-ProductRoot
$addInRoot = Join-Path $productRoot "AddIn"
if (Test-Path -LiteralPath $addInRoot) {
    Remove-Item -LiteralPath $addInRoot -Recurse -Force
}

if ($RemoveUserData -and (Test-Path -LiteralPath $productRoot)) {
    Remove-Item -LiteralPath $productRoot -Recurse -Force
}

Write-Host ""
Write-Host "ExcelApiPoc.AddIn was uninstalled successfully." -ForegroundColor Green
if (-not $RemoveUserData) {
    Write-Host "Settings and cached data were preserved in '$productRoot'."
}

