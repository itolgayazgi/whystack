#Requires -Version 5.1
<#
.SYNOPSIS
    Brings up the local SQL Server, points the API at it, and applies migrations.

.DESCRIPTION
    One command, because a setup that takes four commands is a setup that half-works on someone's
    machine and nobody can reproduce.

    The SA password is GENERATED here, written to infrastructure/docker/.env (gitignored) and to the
    API's user secrets. It is never written to a file that git tracks. The two places always agree
    because one script writes both.

.PARAMETER Reset
    Destroy the database volume first. Everything in the local database is lost.
#>
[CmdletBinding()]
param(
    [switch]$Reset
)

$ErrorActionPreference = 'Stop'

# A PowerShell script that throws leaves $LASTEXITCODE holding the exit code of the last NATIVE command
# it ran — which is usually 0. So a script that died halfway reports success to whatever called it, and
# CI goes green on a setup that never finished. This makes the process exit code tell the truth.
trap {
    Write-Host "`nSetup failed: $_" -ForegroundColor Red
    exit 1
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$composeDir = Join-Path $repoRoot 'infrastructure\docker'
$composeFile = Join-Path $composeDir 'docker-compose.yml'
$envFile = Join-Path $composeDir '.env'
$apiProject = Join-Path $repoRoot 'apps\api\src\WhyStack.Api'

function Write-Step { param([string]$Message) Write-Host "`n==> $Message" -ForegroundColor Cyan }
function Write-Ok { param([string]$Message) Write-Host "    $Message" -ForegroundColor Green }

# Native tools write progress to stderr — docker pull does it constantly. Under
# $ErrorActionPreference = 'Stop', PowerShell mistakes that for a terminating error and kills the
# script mid-pull. So native commands run with the preference relaxed, and success is judged the only
# way that is actually reliable: the exit code.
function Invoke-Native {
    param(
        [Parameter(Mandatory)][scriptblock]$Command,
        [Parameter(Mandatory)][string]$FailureMessage
    )
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try { & $Command } finally { $ErrorActionPreference = $previous }
    if ($LASTEXITCODE -ne 0) { throw $FailureMessage }
}

# --- Preconditions ---------------------------------------------------------------------------
Write-Step 'Checking prerequisites'

Invoke-Native { docker info | Out-Null } 'Docker is not running. Start Docker Desktop and try again.'
Write-Ok 'Docker is running'

# --- Password --------------------------------------------------------------------------------
# Reuse the existing one if there is one: regenerating it would leave the running container on the
# old password and the API on the new one, which fails in a confusing way.
if (Test-Path $envFile) {
    $existing = (Get-Content $envFile | Where-Object { $_ -match '^MSSQL_SA_PASSWORD=' }) -replace '^MSSQL_SA_PASSWORD=', ''
    if ($existing -and $existing -ne 'REPLACE_ME_RUN_THE_SETUP_SCRIPT') {
        $password = $existing
        Write-Step 'Reusing the password already in infrastructure/docker/.env'
    }
}

if (-not $password) {
    Write-Step 'Generating a SQL Server password'
    # SQL Server rejects anything without upper, lower, digit and symbol, so build it from the parts
    # rather than hoping a random string happens to contain all four.
    $upper = -join ((65..90) | Get-Random -Count 6 | ForEach-Object { [char]$_ })
    $lower = -join ((97..122) | Get-Random -Count 6 | ForEach-Object { [char]$_ })
    $digit = -join ((48..57) | Get-Random -Count 4 | ForEach-Object { [char]$_ })
    $symbol = -join (('!', '#', '%', '&', '*', '-', '+', '=', '?' ) | Get-Random -Count 3)
    $password = -join (($upper + $lower + $digit + $symbol).ToCharArray() | Get-Random -Count 19)

    "MSSQL_SA_PASSWORD=$password" | Set-Content -Path $envFile -Encoding utf8
    Write-Ok 'Written to infrastructure/docker/.env (gitignored)'
}

# --- Container -------------------------------------------------------------------------------
if ($Reset) {
    Write-Step 'Destroying the existing database volume (-Reset)'
    Invoke-Native { docker compose -f $composeFile down -v } 'docker compose down failed.'
}

Write-Step 'Starting SQL Server'
Invoke-Native { docker compose -f $composeFile up -d } 'docker compose up failed.'

# "Container started" is not "SQL Server accepts connections" — it is roughly 30 seconds short of it.
# Wait on the container's own healthcheck instead of sleeping and hoping.
Write-Step 'Waiting for SQL Server to accept connections'
$deadline = (Get-Date).AddMinutes(3)
do {
    $status = docker inspect --format '{{.State.Health.Status}}' whystack-sqlserver 2>$null
    if ($status -eq 'healthy') { break }
    if ((Get-Date) -gt $deadline) {
        docker compose -f $composeFile logs --tail 30 sqlserver
        throw "SQL Server did not become healthy within 3 minutes (last status: '$status'). Logs above."
    }
    Start-Sleep -Seconds 5
} while ($true)
Write-Ok 'SQL Server is healthy'

# --- API configuration -----------------------------------------------------------------------
# User secrets, not appsettings.json: the connection string carries a password, and a password in a
# tracked file is a password in the git history forever (CLAUDE.md 1.4).
Write-Step 'Storing the connection string in user secrets'

# TrustServerCertificate is set because the container ships a self-signed certificate. That is
# acceptable for a database bound to 127.0.0.1 on a developer machine, and NOT acceptable anywhere
# else — production gets a real certificate, and this flag must not follow it there.
$connectionString = "Server=127.0.0.1,1433;Database=WhyStack;User Id=sa;Password=$password;Encrypt=True;TrustServerCertificate=True"

Invoke-Native { dotnet user-secrets init --project $apiProject | Out-Null } 'Failed to initialise user secrets.'
Invoke-Native {
    dotnet user-secrets set 'ConnectionStrings:WhyStackDatabase' $connectionString --project $apiProject | Out-Null
} 'Failed to write the connection string to user secrets.'
Write-Ok 'Stored (outside the repository — nothing to commit)'

# --- JWT signing key ---------------------------------------------------------------------------
# Anyone holding this key can mint a valid token for any user, including an Administrator. It is
# generated here, per machine, and written to user secrets — never to appsettings, which is tracked.
Write-Step 'Generating a JWT signing key'

$existingKey = (dotnet user-secrets list --project $apiProject 2>$null) `
    | Where-Object { $_ -match '^Jwt:SigningKey = ' }

if ($existingKey) {
    Write-Ok 'Reusing the key already in user secrets (rotating it would sign everyone out)'
}
else {
    # 64 bytes from a CSPRNG. HMAC-SHA256's security is bounded by the key, and a key someone typed is
    # a key someone can guess.
    #
    # ::Create().GetBytes(), not ::Fill(). Windows PowerShell 5.1 runs on .NET Framework, where Fill()
    # does not exist — it is a .NET Core API. The script is the one thing here that has to work on a
    # machine we have not seen.
    $bytes = [byte[]]::new(64)
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    $signingKey = [Convert]::ToBase64String($bytes)

    Invoke-Native {
        dotnet user-secrets set 'Jwt:SigningKey' $signingKey --project $apiProject | Out-Null
    } 'Failed to write the JWT signing key to user secrets.'

    Write-Ok 'Generated and stored (outside the repository)'
}

# --- Migrations ------------------------------------------------------------------------------
Write-Step 'Applying migrations'
Push-Location (Join-Path $repoRoot 'apps\api')
try {
    Invoke-Native { dotnet tool restore | Out-Null } 'dotnet tool restore failed.'
    Invoke-Native {
        dotnet dotnet-ef database update --project src\WhyStack.Infrastructure --startup-project src\WhyStack.Api
    } 'dotnet ef database update failed.'
}
finally {
    Pop-Location
}

Write-Host "`nDone. The database is up and migrated." -ForegroundColor Green
Write-Host "  Start the API:   pnpm api"
Write-Host "  Check it:        curl http://localhost:5xxx/health/ready"
Write-Host "  Stop the DB:     docker compose -f infrastructure/docker/docker-compose.yml down"
