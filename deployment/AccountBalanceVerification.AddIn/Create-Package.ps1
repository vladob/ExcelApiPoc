param(
    [ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version = '0.1.12'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Push-Location $repo
try {
    $commit = git rev-parse HEAD
    if ($LASTEXITCODE -ne 0) { throw 'Cannot determine Git commit.' }
    $changes = @(git status --porcelain)
    if ($LASTEXITCODE -ne 0 -or $changes.Count -gt 0) { throw 'Commit or stash local changes before packaging.' }
    & dotnet test .\AccountBalanceVerification.Tests\AccountBalanceVerification.Tests.csproj -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
    & dotnet build .\AccountBalanceVerification.AddIn\AccountBalanceVerification.AddIn.csproj -c Release --no-incremental "-p:Version=$Version"
    if ($LASTEXITCODE -ne 0) { throw 'Release build failed.' }
    $output = Join-Path $repo 'AccountBalanceVerification.AddIn\bin\Release\net48'
    $xll = 'AccountBalanceVerification.AddIn-AddIn64.xll'
    foreach ($name in @($xll, "$xll.config", 'AccountBalanceVerification.AddIn.dll', 'System.Text.Json.dll')) {
        if (!(Test-Path -LiteralPath (Join-Path $output $name))) { throw "Missing build output: $name" }
    }
    $dll = Join-Path $output 'AccountBalanceVerification.AddIn.dll'
    if ([Reflection.AssemblyName]::GetAssemblyName($dll).Version.ToString(3) -ne $Version) { throw 'Assembly/package version mismatch.' }
    $config = Get-Content -LiteralPath (Join-Path $output "$xll.config") -Raw
    if ($config -notmatch 'System.Text.Json') { throw 'JSON binding redirect is missing.' }
    $artifacts = Join-Path $PSScriptRoot 'artifacts'
    $name = "AccountBalanceVerification.AddIn-$Version-x64"
    $folder = Join-Path $artifacts $name
    $zip = "$folder.zip"
    if ((Test-Path -LiteralPath $folder) -or (Test-Path -LiteralPath $zip)) { throw 'Package already exists. Move it aside before rebuilding.' }
    $payload = Join-Path $folder 'payload'
    New-Item -ItemType Directory -Path $payload -Force | Out-Null
    # Keep the unpacked XLL, DNA and all managed/native runtime dependencies together.
    Get-ChildItem -LiteralPath $output -File | Where-Object {
        $_.Extension -in @('.dll', '.config', '.dna', '.xll') -and $_.Name -notlike '*-packed.xll*'
    } | Copy-Item -Destination $payload
    foreach ($file in @('Install.cmd','Install.ps1','Uninstall.cmd','Uninstall.ps1','Deployment.Common.ps1','README.txt')) {
        Copy-Item -LiteralPath (Join-Path $PSScriptRoot $file) -Destination $folder
    }
    $files = @(Get-ChildItem -LiteralPath $payload -File | Sort-Object Name | ForEach-Object {
        [ordered]@{ Name = $_.Name; Sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
    })
    [ordered]@{ Product = 'AccountBalanceVerification.AddIn'; Version = $Version; Architecture = 'x64'; SourceCommit = "$commit"; Files = $files } |
        ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $folder 'release.json') -Encoding UTF8
    Compress-Archive -Path (Join-Path $folder '*') -DestinationPath $zip -CompressionLevel Optimal
    Write-Host "Package: $zip" -ForegroundColor Green
    Get-FileHash -LiteralPath $zip -Algorithm SHA256
} finally { Pop-Location }
