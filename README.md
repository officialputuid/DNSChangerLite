<div align="center">

# DNS Changer Lite

**Lightweight DNS changer for Windows 10/11 — one click, done.**

[![.NET](https://img.shields.io/badge/.NET%20Framework-4.8-5C2D91)](https://dotnet.microsoft.com)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D4)](https://www.microsoft.com/windows)
[![Size](https://img.shields.io/badge/Size-~25%20KB-success)](#)
[![License](https://img.shields.io/badge/License-MIT-blue)](LICENSE)
[![CI](https://img.shields.io/badge/CI-manual%20dispatch-yellow)](.github/workflows/build.yml)

*No installation. No runtime dependency. Single portable EXE.*

</div>

---

## Features

| Feature | Description |
|:---|:---|
| **One-click DNS** | Apply preset or custom DNS without opening Control Panel or `netsh` |
| **Built-in presets** | Cloudflare · Google · Quad9 · AdGuard (No Ads) · OpenDNS |
| **Custom DNS** | Enter any IPv4 primary + secondary addresses |
| **Auto DHCP** | Instantly revert to automatic (DHCP) DNS |
| **Flush cache** | Clear the Windows DNS resolver cache |
| **Adapter detection** | Auto-lists connected physical network adapters |
| **Live status** | Shows the active DNS servers per adapter |
| **Native design** | Windows 11 Settings-style UI — rounded cards, Segoe UI, clean spacing |

## Download

Grab the latest `DnsChangerLite.exe` from **[Releases](../../releases/latest)**.

| Property | Value |
|:---|:---|
| File | `DnsChangerLite-v1.0.0.exe` |
| Size | ~25 KB |
| Type | Portable (single file, no installer) |
| Checksum | `sha256.txt` included in every release |

## Getting Started

### Requirements

- **OS:** Windows 10 or Windows 11
- **Runtime:** .NET Framework 4.8 *(pre-installed on all supported Windows versions — nothing to install)*
- **Privileges:** Administrator *(UAC prompt appears automatically — elevation manifest is embedded)*

### Usage

```text
1. Run DnsChangerLite.exe          → UAC elevation prompt
2. Select your network adapter     → auto-detected
3. Pick a preset or Custom         → addresses auto-fill
4. Click Apply DNS                 → done
```

Or click **Auto DHCP** to revert, **Flush DNS** to clear the cache.

## Build from Source

Requires [.NET SDK 8.0+](https://dotnet.microsoft.com/download):

```bash
git clone https://github.com/officialputuid/dns-changer-lite.git
cd dns-changer-lite
dotnet restore DnsChangerLite.sln
dotnet build src/DnsChangerLite/DnsChangerLite.csproj -c Release
```

Output: `src/DnsChangerLite/bin/Release/net48/DnsChangerLite.exe`

Run the test suite:

```bash
dotnet test tests/DnsChangerLite.Tests/DnsChangerLite.Tests.csproj -c Release
```

## Architecture

```text
src/DnsChangerLite/
├── Program.cs           Entry point
├── MainForm.cs          Windows 11-style UI (cards, rounded panels)
├── DnsService.cs        DNS operations (WMI + netsh/ipconfig)
├── app.manifest         UAC elevation + PerMonitorV2 DPI awareness
└── app.ico              Application icon
tests/
└── DnsChangerLite.Tests NUnit test suite
```

**Security hardening:**

- `netsh.exe` / `ipconfig.exe` resolved from the absolute system directory (prevents binary planting)
- No `cmd.exe` shelling — direct process invocation with captured stdout/stderr
- Input validation before any privileged operation
- Partial-success handling (primary vs secondary DNS reported separately)

## Developer

Built and maintained by [officialputuid](https://github.com/officialputuid).
