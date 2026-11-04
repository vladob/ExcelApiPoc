. (Join-Path $PSScriptRoot 'Deployment.Common.ps1')
Assert-ExcelClosed
Assert-64BitMicrosoft365
$release = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'release.json') -Raw | ConvertFrom-Json
if ($release.Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Invalid package version.' }
$payload = Join-Path $PSScriptRoot 'payload'
foreach ($file in $release.Files) {
    if ([IO.Path]::GetFileName($file.Name) -ne $file.Name) { throw 'Invalid payload filename.' }
    $path = Join-Path $payload $file.Name
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $file.Sha256) { throw "Package damaged: $($file.Name)" }
}
$destination = Join-Path (Get-ProductRoot) "AddIn\$($release.Version)"
New-Item -ItemType Directory -Path $destination -Force | Out-Null
foreach ($file in $release.Files) {
    $target = Join-Path $destination $file.Name
    Copy-Item -LiteralPath (Join-Path $payload $file.Name) -Destination $target -Force
    Unblock-File -LiteralPath $target
}
$xll = Join-Path $destination 'AccountBalanceVerification.AddIn-AddIn64.xll'
if (!(Test-Path -LiteralPath "$xll.config")) { throw 'XLL configuration is missing.' }
$null = Set-ExcelAddInRegistration -XllPath $xll
Write-Host 'Inventarizacia installed successfully. Start Excel.' -ForegroundColor Green
Write-Host 'Enter the API server URL and your own API key in the creation dialog.'
