param([string]$MSBuildPath)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Push-Location $repo
try {
    if ([string]::IsNullOrWhiteSpace($MSBuildPath)) {
        $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
        if (!(Test-Path $vswhere)) { throw 'Visual Studio Build Tools with the .NET Framework 4.8 targeting pack are required. Supply -MSBuildPath if installed elsewhere.' }
        $MSBuildPath = (& $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1)
    }
    if (!$MSBuildPath -or !(Test-Path $MSBuildPath)) { throw 'MSBuild.exe was not found.' }
    & dotnet test PdfLayoutEngine.SimpleTests/PdfLayoutEngine.SimpleTests.csproj -c Release --logger 'trx;LogFileName=staged-release.trx'
    if ($LASTEXITCODE -ne 0) { throw 'Staged engine/runtime tests failed.' }
    & dotnet test ExcelApiPoc.AccountingImport.Tests/ExcelApiPoc.AccountingImport.Tests.csproj -c Release --logger 'trx;LogFileName=accounting-release.trx'
    if ($LASTEXITCODE -ne 0) { throw 'Accounting regression tests failed.' }
    & $MSBuildPath ExcelApiPoc.AddIn/ExcelApiPoc.AddIn.csproj /restore /t:Rebuild /p:RestorePackagesConfig=true /p:Configuration=Release /nologo
    if ($LASTEXITCODE -ne 0) { throw 'AddIn Release rebuild failed.' }
    $output = Join-Path $repo 'ExcelApiPoc.AddIn\bin\Release'
    $dll = Join-Path $output 'ExcelApiPoc.AddIn.dll'
    $version = [Reflection.AssemblyName]::GetAssemblyName($dll).Version.ToString()
    if ($version -ne '1.1.4.0') { throw "Validation phase expects assembly 1.1.4.0; found $version." }
    [xml]$dna = Get-Content (Join-Path $repo 'ExcelApiPoc.AddIn\ExcelApiPoc.AddIn-AddIn.dna') -Raw
    $packed = @($dna.SelectNodes('//*[@Pack="true"]') | ForEach-Object { $_.Path })
    foreach ($name in $packed) {
        if (!(Test-Path (Join-Path $output $name))) { throw "Packed dependency missing from build output: $name" }
    }
    # These are supplied by Excel-DNA/Office or the Framework, not packed dependencies.
    $external = @('ExcelDna.Integration.dll', 'Microsoft.Office.Interop.Excel.dll', 'office.dll', 'stdole.dll')
    $unlisted = @(Get-ChildItem $output -Filter '*.dll' | Where-Object { $_.Name -notin $packed -and $_.Name -notin $external })
    if ($unlisted.Count -gt 0) { throw ('Build output has DLLs absent from the packing manifest; review before testing: ' + ($unlisted.Name -join ', ')) }
    $xll = Join-Path $output 'ExcelApiPoc.AddIn-AddIn64-packed.xll'
    if (!(Test-Path $xll) -or (Get-Item $xll).Length -eq 0) { throw 'Packed x64 XLL was not produced.' }
    $smoke = Join-Path ([IO.Path]::GetTempPath()) ('ExcelApiPoc-smoke-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $smoke | Out-Null
    $isolatedXll = Join-Path $smoke 'ExcelApiPoc.AddIn.xll'
    Copy-Item $xll $isolatedXll
    Write-Host "Assembly version: $version"
    Write-Host ('Commit: ' + (git rev-parse HEAD))
    Write-Host ('XLL SHA256: ' + (Get-FileHash $isolatedXll -Algorithm SHA256).Hash)
    Write-Host "Isolated XLL for Excel smoke test: $isolatedXll"
    Write-Host 'Automated checks complete. The packed XLL has NOT yet been tested inside Excel.'
} finally { Pop-Location }
