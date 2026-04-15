# 🌤️ Mr. YAN
### Meteorological Responses Your Accurate Notification

Monitors NWS weather alerts for **Monroe County, IN** and posts them to Google Chat.
Features a self-contained native GUI — no separate browser needed.

---

## How the GUI works

| Platform | GUI mode | Fallback |
|---|---|---|
| **Windows** (run normally) | Native window (WebView2, built into Windows 10/11) | — |
| **Raspberry Pi** with desktop + display | Native window (WebKitGTK) | — |
| **Raspberry Pi** as headless service | No window — runs as background service | Access via `http://pi-ip:5000` |

When running as a Windows Service or systemd unit, the `--headless` flag is set automatically and no window opens.

---

## Requirements

**Build machine:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

**Windows (GUI mode):** WebView2 runtime — already installed with Windows 10/11 and Microsoft Edge. If missing: [download here](https://developer.microsoft.com/en-us/microsoft-edge/webview2/).

**Raspberry Pi (GUI mode, optional):** WebKitGTK — install once on the Pi:
```bash
sudo apt install libwebkit2gtk-4.1-dev
# Or for older Raspbian:
sudo apt install libwebkit2gtk-4.0-dev
```
If neither is installed, Mr. YAN runs headlessly and the GUI is accessible via browser at `http://localhost:5000`.

---

## Two-step workflow

### Step 1 — Build

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\build.ps1
```

Single target:
```powershell
.\build.ps1 -Target pi64      # Pi 3/4/5 (64-bit Raspbian)
.\build.ps1 -Target pi32      # Pi 2 or older (32-bit)
.\build.ps1 -Target windows   # Windows x64
```

### Step 2 — Deploy

```powershell
.\deploy.ps1
```

Prompts for platform, IP, architecture. Or non-interactive:

```powershell
.\deploy.ps1 -Target Pi -PiIP 192.168.1.100
.\deploy.ps1 -Target Windows   # run PowerShell as Administrator
```

---

## Running locally (without deploying as a service)

**Windows:**
```powershell
cd publish\win-x64
.\MRYAN.exe
```
A native window opens. No browser needed.

**Pi (with desktop):**
```bash
cd /path/to/publish
./MRYAN
```

**Headless (any platform):**
```bash
./MRYAN --headless
# Then open http://localhost:5000 in a browser
```

---

## Configuration (`appsettings.json`)

| Key | Default | Description |
|---|---|---|
| `WebhookUrl` | _(pre-filled)_ | Google Chat webhook URL |
| `CheckIntervalMinutes` | `5` | Poll interval (also editable in GUI) |
| `RepostIntervalMinutes` | `60` | Re-post interval (also editable in GUI) |
| `PostAllClear` | `true` | Post when alerts clear |
| `IncludeDescription` | `true` | Include alert body |
| `IncludeInstructions` | `true` | Include safety instructions |
| `MaxDescriptionChars` | `800` | Description truncation length |

---

## Service commands (Pi)

```bash
sudo systemctl status mryan
sudo systemctl restart mryan
journalctl -u mryan -f
sudo nano /opt/mryan/appsettings.json
```

## Service commands (Windows)

```powershell
Get-Service MrYAN
Restart-Service MrYAN
notepad "C:\Services\MrYAN\appsettings.json"
Stop-Service MrYAN; sc.exe delete MrYAN   # uninstall
```
