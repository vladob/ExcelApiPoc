$ErrorActionPreference = 'Stop'
$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Push-Location $repo
try {
    dotnet test .\AccountBalanceVerification.Tests\AccountBalanceVerification.Tests.csproj -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
    dotnet build .\ExcelApiPoc.Api\ExcelApiPoc.Api.csproj -c Release
    if ($LASTEXITCODE -ne 0) { throw 'API build failed.' }
    dotnet build .\AccountBalanceVerification.AddIn\AccountBalanceVerification.AddIn.csproj -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Add-in build failed.' }
    Write-Host 'Open the 64-bit XLL in AccountBalanceVerification.AddIn\bin\Release\net48. Keep the entire output folder together.'
} finally { Pop-Location }
