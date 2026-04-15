#!/usr/bin/env pwsh
param(
    [ValidateSet("all","windows","pi64","pi32")]
    [string]$Target = "all"
)
$ErrorActionPreference = "Stop"
$project = "MRYAN.csproj"

function Publish($rid, $framework, $label) {
    Write-Host "`n==> Building for $label ($rid)..." -ForegroundColor Cyan
    dotnet publish $project `
        --configuration Release `
        --runtime $rid `
        --framework $framework `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=false `
        --output ".\publish\$rid"
    if ($LASTEXITCODE -ne 0) { Write-Error "Build failed for $rid"; exit 1 }
    Write-Host "    Output: .\publish\$rid\" -ForegroundColor Green
}

if ($Target -eq "all" -or $Target -eq "windows") { Publish "win-x64"     "net8.0-windows" "Windows x64 (GUI)"      }
if ($Target -eq "all" -or $Target -eq "pi64")    { Publish "linux-arm64" "net8.0"         "Raspberry Pi 64-bit"     }
if ($Target -eq "all" -or $Target -eq "pi32")    { Publish "linux-arm"   "net8.0"         "Raspberry Pi 32-bit"     }

Write-Host "`n✅ Build complete." -ForegroundColor Green
