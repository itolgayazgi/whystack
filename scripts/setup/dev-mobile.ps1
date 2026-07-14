#Requires -Version 5.1
<#
.SYNOPSIS
    Puts the app on a real phone — the fast human feedback loop.

.DESCRIPTION
    Real fonts, real keyboard, real gestures, real Turkish text overflowing a real button. None of which
    CI can see, and none of which I can see. This is the loop that answers "does it feel right?".

    IT USES A DEVELOPMENT BUILD, NOT EXPO GO.

      Expo Go ships a FIXED set of native modules, compiled against one Expo SDK. The published version
      supports SDK 54; this project is on SDK 57, so it refuses to open the project at all. There is
      nothing to be done about that from here.

      Which is just as well, because Expo Go was always the weaker tool. It is EXPO'S app: its bundle
      identifier, its signature and its entitlements belong to Expo, so it could never have told us
      anything about the binary we actually ship. The iCloud-sync bug we found in the refresh token
      would not have appeared there — SecureStore writes under Expo Go's keychain entitlements rather
      than ours.

      A development build is OUR app: our bundle id, our signature, our entitlements. It still loads its
      JavaScript from Metro at runtime, so a code change appears on the phone in a second — the same
      loop Expo Go offered, with none of its blind spots.

      Get the APK from the "Development build" workflow in GitHub Actions. Install it once; re-install
      only when a NATIVE dependency changes.

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

# ── Now tell the API about the phone ───────────────────────────────────────────────────────────────
#
# User secrets, not appsettings: these are one developer's private network address, and they have no
# business in a tracked file.
$apiProject = Join-Path $repoRoot 'apps\api\src\WhyStack.Api'
$clientBaseUrl = "http://${lanIp}:8081"

# 1. WHERE THE EMAIL LINKS POINT.
#
# The confirmation and reset emails contain a URL, and a human clicks it — on the phone that received
# the email. The default is http://localhost:8081, which that phone cannot reach: it resolves localhost
# to ITSELF, finds nothing, and the link is dead.
#
# Metro serves the web build at this address, so the link opens the web app in the phone's browser,
# which confirms the account against the API. Opening the NATIVE app directly needs Universal Links —
# a verification file on a real domain, plus an Apple team — which we do not have yet.
# The email links point at the WEBSITE (ADR-0022), not at Metro.
#
# /confirm-email and /reset-password live in apps/web. The mobile app has no deep-link handler, and a
# confirmation link is clicked from a mail client — often on a laptop, where Metro is not even running.
dotnet user-secrets set 'App:ClientBaseUrl' 'http://localhost:3000' --project $apiProject | Out-Null

# 2. CORS — AND THIS ONE COST AN EVENING.
#
# That browser page is a DIFFERENT ORIGIN. http://192.168.1.101:8081 is not http://localhost:8081, and
# the API's CORS policy names origins exactly — it has to, because a browser refuses a wildcard origin
# alongside credentials. (That refusal is a feature: without it, any site on the internet could make
# authenticated calls to this API on a logged-in user's behalf.)
#
# Without this, the browser blocks the request BEFORE IT LEAVES THE PHONE. The app cannot tell that
# apart from a dead network, so it says "Cannot reach WhyStack" — an honest message about a cause it
# has no way of seeing. Which is precisely what happened, and what this script now prevents.
#
# EVERY origin is written, every time — and that is not belt-and-braces, it is a bug fix.
#
# .NET configuration providers do not REPLACE an array. They MERGE IT BY INDEX. user-secrets sits above
# appsettings.json, so writing `Cors:AllowedOrigins:0` overwrites element 0 of whatever appsettings.json
# declared — and element 2 survives untouched, which makes the result look like a merge and behave like a
# partial overwrite.
#
# An earlier version of this script wrote only indices 0 and 1. appsettings.json had the WEBSITE at index 0.
# The website's origin was silently deleted, the browser stopped getting Access-Control-Allow-Origin, and
# the sign-in page said "cannot reach the server" — which was true, and pointed nowhere near the cause.
#
# So this list is the WHOLE list. If a surface needs an origin, it is here.
$origins = @(
    'http://localhost:3000',        # the website, on this machine
    "http://${lanIp}:3000",         # the website, from the phone's browser
    'http://localhost:8081',        # Metro, on this machine
    $clientBaseUrl                  # Metro, from the phone
)

for ($i = 0; $i -lt $origins.Count; $i++) {
    dotnet user-secrets set "Cors:AllowedOrigins:$i" $origins[$i] --project $apiProject | Out-Null
}

Write-Host "  Email links point at   $clientBaseUrl" -ForegroundColor Green
Write-Host "  CORS allows            $($origins -join ', ')" -ForegroundColor Green

# ── The firewall ───────────────────────────────────────────────────────────────────────────────────
#
# Windows blocks inbound connections to a port nothing has asked it to open. The phone's request is
# dropped silently — no error on either side, the app simply shows its offline state, and there is
# nothing anywhere to explain why. It is the single most likely thing to go wrong here.
#
# TWO ports, and forgetting the second is the classic mistake:
#
#   5207  the API. Without it the app opens and then cannot sign anybody in.
#   8081  Metro — the JavaScript bundler. Without it the app does not open AT ALL: it scans the QR,
#         tries to fetch the bundle from this machine, and hangs on a white screen. That looks like a
#         broken build rather than a blocked port.
$rule = Get-NetFirewallRule -DisplayName 'WhyStack dev' -ErrorAction SilentlyContinue

if ($null -eq $rule) {
    Write-Host ''
    Write-Host '  The Windows firewall will drop the phone''s requests until two ports are open.' -ForegroundColor Yellow
    Write-Host '  It fails SILENTLY: no error on either side, and nothing says why.' -ForegroundColor Yellow
    Write-Host ''
    Write-Host '    5207  the API   - without it, the app opens but nobody can sign in' -ForegroundColor DarkGray
    Write-Host '    8081  Metro     - without it, the app does not open at all (white screen)' -ForegroundColor DarkGray
    Write-Host ''
    Write-Host '  Run this ONCE, in a PowerShell started as Administrator:' -ForegroundColor Yellow
    Write-Host ''
    Write-Host '    New-NetFirewallRule -DisplayName "WhyStack dev" -Direction Inbound `' -ForegroundColor White
    Write-Host '      -Protocol TCP -LocalPort 5207,8081 -Action Allow -Profile Private' -ForegroundColor White
    Write-Host ''
    Write-Host '  -Profile Private: your home Wi-Fi, not a coffee shop''s. The rule should not follow' -ForegroundColor DarkGray
    Write-Host '  you onto a public network and quietly expose a development API to strangers.' -ForegroundColor DarkGray
} else {
    Write-Host '  Firewall rule for ports 5207 and 8081 is already in place.' -ForegroundColor Green
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
Write-Host 'Then open WhyStack on your phone and scan the QR code.' -ForegroundColor Cyan
Write-Host 'The phone and this machine must be on the SAME Wi-Fi.' -ForegroundColor Cyan
Write-Host ''
Write-Host 'No app on the phone yet? Download the APK from GitHub Actions:' -ForegroundColor Cyan
Write-Host '  Actions > Development build > the newest run > whystack-android-dev-build' -ForegroundColor DarkGray
Write-Host 'Install it once. Re-install only when a NATIVE dependency changes.' -ForegroundColor DarkGray
Write-Host ''
Write-Host 'Read the mail the app sends at  http://localhost:8025  (Mailpit).' -ForegroundColor DarkGray
Write-Host 'Confirmation and reset links point at the web client, not the phone. Deep linking into' -ForegroundColor DarkGray
Write-Host 'the app is not built yet, so open those links in a browser.' -ForegroundColor DarkGray
Write-Host ''
