. (Join-Path $PSScriptRoot 'Deployment.Common.ps1')
Assert-ExcelClosed
Remove-ExcelAddInRegistration
$installRoot = Join-Path (Get-ProductRoot) 'AddIn'
if (Test-Path -LiteralPath $installRoot) { Remove-Item -LiteralPath $installRoot -Recurse -Force }
Write-Host 'Inventarizacia uninstalled. User settings and workbooks were preserved.'
