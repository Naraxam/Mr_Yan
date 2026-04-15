#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Deploys Mr. YAN to Windows (as a Service) or a Raspberry Pi (via SSH).

.DESCRIPTION
    Run AFTER build.ps1. Detects your target platform and walks you through
    deployment automatically. Can also be driven non-interactively with params.

.EXAMPLES
    # Interactive — prompts you for everything:
    .\deploy.ps1

    # Deploy to Pi non-interactively:
    .\deploy.ps1 -Target Pi -PiIP 192.168.1.100

    # Deploy to Pi with 32-bit arch and custom user:
    .\deploy.ps1 -Target Pi -PiIP 192.168.1.100 -Arch arm -PiUser myuser

    # Install as Windows Service (must be run as Administrator):
    .\deploy.ps1 -Target Windows
#>

param(
    [ValidateSet("Windows","Pi","")]
    [string]$Target = "",

    # Pi options
    [string]$PiIP   = "",
    [string]$PiUser = "pi",
    [ValidateSet("arm64","arm")]
    [string]$Arch   = "arm64",

    # Windows options
    [string]$InstallDir = "C:\Services\MrYAN"
)

$ErrorActionPreference = "Stop"

# ── Banner ────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  🌤️  Mr. YAN Deployment Utility" -ForegroundColor Cyan
Write-Host "  Meteorological Responses Your Accurate Notification" -ForegroundColor DarkCyan
Write-Host ""

# ── Choose target interactively if not supplied ───────────────────────────────
if ($Target -eq "") {
    Write-Host "Where do you want to deploy Mr. YAN?" -ForegroundColor Yellow
    Write-Host "  [1] Raspberry Pi  (SSH deploy + systemd service)"
    Write-Host "  [2] Windows       (Windows Service — run as Administrator)"
    Write-Host ""
    $choice = Read-Host "Enter 1 or 2"
    $Target = if ($choice -eq "2") { "Windows" } else { "Pi" }
}

# =============================================================================
# WINDOWS
# =============================================================================
if ($Target -eq "Windows") {

    # Check for admin
    $isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
                 [Security.Principal.WindowsBuiltInRole]::Administrator)
    if (-not $isAdmin) {
        Write-Error "Windows Service install requires Administrator. Right-click PowerShell → 'Run as Administrator' and re-run."
        exit 1
    }

    $rid    = "win-x64"
    $binDir = ".\publish\$rid"
    $exe    = "$binDir\MRYAN.exe"

    if (-not (Test-Path $exe)) {
        Write-Error "No Windows build found at $exe  →  Run  .\build.ps1 -Target windows  first."
        exit 1
    }

    $serviceName = "MrYAN"
    $displayName = "Mr. YAN – Meteorological Responses Your Accurate Notification"

    # Stop and remove old service if present
    $existing = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($existing) {
        Write-Host "Removing existing service…" -ForegroundColor Yellow
        Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
        sc.exe delete $serviceName | Out-Null
        Start-Sleep -Seconds 1
    }

    # Copy files
    Write-Host "Installing to $InstallDir…"
    New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
    Copy-Item "$binDir\*" $InstallDir -Force

    # Register
    Write-Host "Registering Windows Service…"
    New-Service `
        -Name           $serviceName `
        -DisplayName    $displayName `
        -Description    "Monitors NWS weather alerts for Monroe County, IN and posts to Google Chat." `
        -BinaryPathName "$InstallDir\MRYAN.exe" `
        -StartupType    Automatic | Out-Null

    # Recovery: restart on failure (15s, 30s, 60s)
    sc.exe failure $serviceName reset= 86400 actions= restart/15000/restart/30000/restart/60000 | Out-Null

    # Start
    Write-Host "Starting service…"
    Start-Service -Name $serviceName
    Start-Sleep -Seconds 3

    $status = (Get-Service -Name $serviceName).Status
    if ($status -eq "Running") {
        Write-Host ""
        Write-Host "✅ Mr. YAN is running as a Windows Service." -ForegroundColor Green
    } else {
        Write-Warning "Service status: $status — check Event Viewer → Windows Logs → Application (source: MrYAN)"
    }

    Write-Host ""
    Write-Host "Useful commands:" -ForegroundColor Cyan
    Write-Host "  Status:    Get-Service MrYAN"
    Write-Host "  Restart:   Restart-Service MrYAN"
    Write-Host "  Stop:      Stop-Service MrYAN"
    Write-Host "  Config:    notepad '$InstallDir\appsettings.json'  (restart after editing)"
    Write-Host "  Uninstall: Stop-Service MrYAN; sc.exe delete MrYAN"
}

# =============================================================================
# RASPBERRY PI
# =============================================================================
elseif ($Target -eq "Pi") {

    # Gather Pi IP interactively if not provided
    if ($PiIP -eq "") {
        Write-Host "Raspberry Pi deployment" -ForegroundColor Yellow
        Write-Host ""
        $PiIP = Read-Host "Pi IP address (e.g. 192.168.1.100)"
    }

    # Ask architecture interactively if not driven by param
    if (-not $PSBoundParameters.ContainsKey("Arch")) {
        Write-Host ""
        Write-Host "Pi architecture?" -ForegroundColor Yellow
        Write-Host "  [1] 64-bit  (Pi 3 / 4 / 5 running 64-bit Raspbian OS)  ← most common"
        Write-Host "  [2] 32-bit  (Pi 2 or older, or 32-bit Raspbian OS)"
        $archChoice = Read-Host "Enter 1 or 2 [default: 1]"
        $Arch = if ($archChoice -eq "2") { "arm" } else { "arm64" }
    }

    $rid     = "linux-$Arch"
    $binDir  = ".\publish\$rid"
    $binary  = "$binDir\MRYAN"
    $config  = "$binDir\appsettings.json"
    $service = ".\mryan.service"
    $dest    = "$PiUser@$PiIP"

    Write-Host ""
    Write-Host "Deploying to $dest  (arch: $Arch)" -ForegroundColor Cyan

    # Check files
    foreach ($f in @($binary, $config, $service)) {
        if (-not (Test-Path $f)) {
            $hint = if ($Arch -eq "arm64") { "pi64" } else { "pi32" }
            Write-Error "Missing: $f  →  Run  .\build.ps1 -Target $hint  first."
            exit 1
        }
    }

    # Copy files to Pi
    Write-Host "Copying files to Pi…"
    scp $binary  "${dest}:/tmp/MRYAN"
    scp $config  "${dest}:/tmp/appsettings.json"
    scp $service "${dest}:/tmp/mryan.service"

    # Remote setup script
    Write-Host "Running setup on Pi…"
    $setup = @'
set -e
sudo useradd --system --no-create-home --shell /sbin/nologin mryan 2>/dev/null || true
sudo mkdir -p /opt/mryan

sudo mv /tmp/MRYAN          /opt/mryan/MRYAN
sudo mv /tmp/appsettings.json /opt/mryan/appsettings.json
sudo chmod +x  /opt/mryan/MRYAN
sudo chmod 640 /opt/mryan/appsettings.json
sudo chown -R mryan:mryan /opt/mryan

sudo timedatectl set-timezone America/Indiana/Indianapolis
sudo timedatectl set-ntp true

sudo mv /tmp/mryan.service /etc/systemd/system/mryan.service
sudo systemctl daemon-reload
sudo systemctl enable mryan
sudo systemctl restart mryan

echo ""
echo "--- Service status ---"
sudo systemctl status mryan --no-pager
'@

    ssh $dest $setup

    Write-Host ""
    Write-Host "✅ Mr. YAN deployed and running on $PiIP." -ForegroundColor Green
    Write-Host ""
    Write-Host "Useful commands:" -ForegroundColor Cyan
    Write-Host "  Live logs:  ssh $dest 'journalctl -u mryan -f'"
    Write-Host "  Status:     ssh $dest 'sudo systemctl status mryan'"
    Write-Host "  Restart:    ssh $dest 'sudo systemctl restart mryan'"
    Write-Host "  Config:     ssh $dest 'sudo nano /opt/mryan/appsettings.json'"
    Write-Host "              (then: ssh $dest 'sudo systemctl restart mryan')"
    Write-Host "  Uninstall:  ssh $dest 'sudo systemctl disable mryan; sudo rm /etc/systemd/system/mryan.service; sudo rm -rf /opt/mryan'"
}
