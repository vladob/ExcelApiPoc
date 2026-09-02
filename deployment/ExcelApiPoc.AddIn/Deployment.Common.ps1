Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:ProductName = "ExcelApiPoc.AddIn"
$script:OfficeVersion = "16.0"

function Assert-ExcelClosed {
    if (Get-Process -Name EXCEL -ErrorAction SilentlyContinue) {
        throw "Microsoft Excel is running. Close every Excel window and run this command again."
    }
}

function Assert-64BitMicrosoft365 {
    $configurationPaths = @(
        "HKLM:\SOFTWARE\Microsoft\Office\ClickToRun\Configuration",
        "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Office\ClickToRun\Configuration"
    )

    foreach ($configurationPath in $configurationPaths) {
        $configuration = Get-ItemProperty -Path $configurationPath -ErrorAction SilentlyContinue
        if ($null -ne $configuration -and -not [string]::IsNullOrWhiteSpace($configuration.Platform)) {
            if ($configuration.Platform -ne "x64") {
                throw "This package requires 64-bit Excel. Microsoft 365 reports platform '$($configuration.Platform)'."
            }

            return
        }
    }

    throw "The Microsoft 365 architecture could not be detected. Installation stopped without changing Excel."
}

function Get-ExcelOptionsPath {
    return "HKCU:\Software\Microsoft\Office\$script:OfficeVersion\Excel\Options"
}

function Get-ExcelOpenRegistrations {
    $optionsPath = Get-ExcelOptionsPath
    if (-not (Test-Path -LiteralPath $optionsPath)) {
        return @()
    }

    $properties = Get-ItemProperty -LiteralPath $optionsPath
    return @(
        $properties.PSObject.Properties |
            Where-Object { $_.Name -match '^OPEN\d*$' } |
            ForEach-Object {
                [pscustomobject]@{
                    Name = $_.Name
                    Value = [string]$_.Value
                }
            }
    )
}

function Set-ExcelAddInRegistration {
    param(
        [Parameter(Mandatory = $true)]
        [string]$XllPath
    )

    $resolvedXllPath = (Resolve-Path -LiteralPath $XllPath).Path
    $optionsPath = Get-ExcelOptionsPath
    New-Item -Path $optionsPath -Force | Out-Null

    $registrations = @(Get-ExcelOpenRegistrations)
    $productRegistrations = @(
        $registrations |
            Where-Object { $_.Value -like "*$script:ProductName*" }
    )

    if ($productRegistrations.Count -gt 0) {
        $registrationName = $productRegistrations[0].Name
        foreach ($duplicate in $productRegistrations | Select-Object -Skip 1) {
            Remove-ItemProperty -LiteralPath $optionsPath -Name $duplicate.Name
        }
    }
    else {
        $usedNames = @(
            $registrations |
                ForEach-Object { $_.Name }
        )
        $registrationName = "OPEN"
        $index = 1
        while ($usedNames -contains $registrationName) {
            $registrationName = "OPEN$index"
            $index++
        }
    }

    Set-ItemProperty `
        -LiteralPath $optionsPath `
        -Name $registrationName `
        -Value "/R `"$resolvedXllPath`""

    return $registrationName
}

function Remove-ExcelAddInRegistration {
    $optionsPath = Get-ExcelOptionsPath
    if (-not (Test-Path -LiteralPath $optionsPath)) {
        return
    }

    foreach ($registration in @(Get-ExcelOpenRegistrations)) {
        if ($registration.Value -like "*$script:ProductName*") {
            Remove-ItemProperty -LiteralPath $optionsPath -Name $registration.Name
        }
    }
}

function Get-ProductRoot {
    return Join-Path $env:LOCALAPPDATA "ExcelApiPoc"
}
