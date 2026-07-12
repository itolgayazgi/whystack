#Requires -Version 5.1
<#
.SYNOPSIS
    Puts the app on a real phone, over Expo Go, in about a minute.

.DESCRIPTION
    Expo Go runs the JavaScript of this app inside Expo's own container. It is the fast human feedback
    loop: real fonts, real keyboard, real gestures, real Turkish text overflowing a real button — none
    of which CI can see and none of which I can see.

    WHAT IT DOES NOT PROVE, and this matters:

      Expo Go is EXPO'S app, not ours. Its bundle identifier, its signature and its entitlements are
      Expo's. So it cannot tell you anything about OUR binary — the one a user would install. The
      iCloud-sync bug we found in the refresh token would NOT have appeared here, because SecureStore
      writes under Expo Go's keychain entitlements rather than ours. Nothing in app.config.js's native
      configuration applies either.

      Expo Go answers "does this feel right?". The native CI workflow answers "does the thing we ship
      actually work?". They are different questions. Keep both.

    THE ONE REAL OBSTACLE this script exists to remove: a phone cannot reach `localhost`. The API binds
    to the loopback interface by default, which is unreachable from anything but this machine. So the
    app has to be pointed at this machine's address on the local network, and the API has to be
    listening on it.
#>
$ErrorActionPreference = 'Stop'
trap { Write-Host "`n$($_.Exception.Message)" -ForegroundColor Red; exit 1 }

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$apiPort = 5207

Write-Host ''
Write-Host 'WhyStack - mobile development setup' -ForegroundColor Cyan
Write-Host '-----------------------------------' -ForegroundColor Cyan

# ── This machine's address on the local network ────────────────────────────────────────────────────
#
# Not "the first IP address". A developer laptop is full of interfaces that look like network adapters
# and are not: Docker's bridge, WSL's virtual switch, VPN adapters, Hyper-V. Picking one of those gives
# an address the phone cannot reach, and the app lands in its offline state — which looks like a bug in
# the app rather than a bug in this script.
#
# The one that can carry traffic to the phone is the one with a default gateway.
$candidates = Get-NetIPConfiguration |
    Where-Object { $null -ne $_.IPv4DefaultGateway -and $_.NetAdapter.Status -eq 'Up' } |
    Where-Object { $_.InterfaceAlias -notmatch 'Loopback|vEthernet|Docker|WSL' }

$address = $candidates | Select-Object -First 1 -ExpandProperty IPv4Address

if ($null -eq $address) {
    throw @'
Could not find this machine's address on the local network.

Every adapter with a default gateway was filtered out, or none is up. Are you connected to Wi-Fi?
'@
}

$lanIp = $address.IPAddress
$apiUrl = "http://${lanIp}:${apiPort}"

Write-Host ''
Write-Host "  This machine on the network:  $lanIp" -ForegroundColor Green
Write-Host "  The API the phone will call:  $apiUrl" -ForegroundColor Green

# ── Point the app at it ────────────────────────────────────────────────────────────────────────────
#
# A .env file, not an environment variable. EXPO_PUBLIC_* values are inlined into the JavaScript bundle
# at BUILD time, and an environment variable that fails to reach the bundler leaves the app silently
# falling back to localhost — which is exactly the bug that made the Android CI run fail while iOS
# passed for the wrong reason. A file is read by the bundler itself and cannot go missing in transit.
#
# It is gitignored, and it must stay that way: it names a private address on somebody's home network.
$envFile = Join-Path $repoRoot 'apps\client\.env'

$content = @"
# Written by scripts/setup/dev-mobile.ps1 - do not commit (it is gitignored).
#
# Your phone cannot reach localhost. This is this machine's address on your local network, and it is
# only valid while you are on this network. Re-run the script if it changes.
EXPO_PUBLIC_API_URL=$apiUrl
"@

# WriteAllText with an explicit BOM-less encoder, not Set-Content -Encoding utf8.
#
# Windows PowerShell 5.1's "utf8" means UTF-8 WITH a byte-order mark, and a .env parser reading a BOM
# as the first character of a key name does not see EXPO_PUBLIC_API_URL — it sees "﻿EXPO_PUBLIC_API_URL",
# which matches nothing. The variable is silently absent, the app falls back to localhost, and the phone
# shows its offline screen with nothing anywhere to explain why.
#
# The BOM happens to sit on a comment line here, so it would work today and break the moment somebody
# reordered the file. That is not a thing to leave lying around.
[System.IO.File]::WriteAllText($envFile, $content, (New-Object System.Text.UTF8Encoding($false)))

Write-Host ''
Write-Host "  Wrote apps/client/.env" -ForegroundColor Green

# ── The firewall ───────────────────────────────────────────────────────────────────────────────────
#
# Windows blocks inbound connections to a port nothing has asked it to open. The phone's request is
# dropped silently — no error on either side, the app simply shows its offline state, and there is
# nothing anywhere to explain why. It is the single most likely thing to go wrong here.
$rule = Get-NetFirewallRule -DisplayName 'WhyStack dev API' -ErrorAction SilentlyContinue

if ($null -eq $rule) {
    Write-Host ''
    Write-Host '  The Windows firewall will drop the phone''s requests until port 5207 is open.' -ForegroundColor Yellow
    Write-Host '  It fails SILENTLY: the app just shows "offline", and nothing says why.' -ForegroundColor Yellow
    Write-Host ''
    Write-Host '  Run this ONCE, in a PowerShell started as Administrator:' -ForegroundColor Yellow
    Write-Host ''
    Write-Host '    New-NetFirewallRule -DisplayName "WhyStack dev API" -Direction Inbound `' -ForegroundColor White
    Write-Host '      -Protocol TCP -LocalPort 5207 -Action Allow -Profile Private' -ForegroundColor White
    Write-Host ''
    Write-Host '  -Profile Private: your home Wi-Fi, not a coffee shop''s. The rule should not follow' -ForegroundColor DarkGray
    Write-Host '  you onto a public network and quietly expose a development API to strangers.' -ForegroundColor DarkGray
} else {
    Write-Host '  Firewall rule for port 5207 is already in place.' -ForegroundColor Green
}

# ── What to run ────────────────────────────────────────────────────────────────────────────────────
Write-Host ''
Write-Host 'Now, in three terminals:' -ForegroundColor Cyan
Write-Host ''
Write-Host '  1.  The database and the mail catcher' -ForegroundColor White
Write-Host '      docker compose -f infrastructure/docker/docker-compose.yml up -d' -ForegroundColor DarkGray
Write-Host ''
Write-Host '  2.  The API - on every interface, not just loopback' -ForegroundColor White
Write-Host '      pnpm api:lan' -ForegroundColor DarkGray
Write-Host ''
Write-Host '  3.  The app' -ForegroundColor White
Write-Host '      pnpm mobile' -ForegroundColor DarkGray
Write-Host ''
Write-Host 'Then open Expo Go on your phone and scan the QR code.' -ForegroundColor Cyan
Write-Host 'The phone and this machine must be on the SAME Wi-Fi.' -ForegroundColor Cyan
Write-Host ''
Write-Host 'Read the mail the app sends at  http://localhost:8025  (Mailpit).' -ForegroundColor DarkGray
Write-Host 'Confirmation and reset links point at the web client, not the phone. Deep linking into' -ForegroundColor DarkGray
Write-Host 'the app is not built yet, so open those links in a browser.' -ForegroundColor DarkGray
Write-Host ''
