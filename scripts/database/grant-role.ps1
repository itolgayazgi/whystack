#Requires -Version 5.1
<#
.SYNOPSIS
  Grants a role to a local account. Development only.

.DESCRIPTION
  There is no admin screen yet, and the roles that matter for content — Editor, Reviewer, Administrator —
  are the ones that let a person READ a draft. Without one of them, a reviewer cannot see the thing they
  are supposed to review, and `content/` may as well not exist.

  This talks to the DEVELOPMENT database. It is deliberately not an API endpoint: an endpoint that grants
  roles is an endpoint that escalates privilege, and CLAUDE.md §6 is explicit that role escalation is a
  human decision, never an agent's. A script somebody has to run, on their own machine, against their own
  database, is the whole point.

.EXAMPLE
  pnpm role -- -Email you@example.com -Role Administrator
#>

[CmdletBinding()]
param(
  [Parameter(Mandatory)]
  [string] $Email,

  [ValidateSet('Guest', 'RegisteredUser', 'PremiumUser', 'Editor', 'Reviewer', 'Translator', 'Administrator')]
  [string] $Role = 'Administrator'
)

$ErrorActionPreference = 'Stop'

$container = 'whystack-sqlserver'

if (-not (docker ps --filter "name=$container" --format '{{.Names}}')) {
  throw "The $container container is not running. Start it with: docker compose -f infrastructure/docker/docker-compose.yml up -d"
}

# The password comes from user secrets, not from this file. A connection string in a tracked script is a
# connection string in the git history forever.
$secrets = dotnet user-secrets list --project apps/api/src/WhyStack.Api 2>$null
$line = $secrets | Where-Object { $_ -like 'ConnectionStrings:WhyStackDatabase*' }

if (-not $line) {
  throw 'ConnectionStrings:WhyStackDatabase is not in user secrets. Run scripts/setup/dev-database.ps1 first.'
}

if ($line -notmatch 'Password=([^;]+)') {
  throw 'Could not read the password out of the connection string.'
}

$password = $Matches[1]

$sql = @"
SET NOCOUNT ON;

DECLARE @UserId uniqueidentifier = (SELECT Id FROM Users WHERE NormalizedEmail = UPPER('$Email'));
DECLARE @RoleId uniqueidentifier = (SELECT Id FROM Roles WHERE Name = '$Role');

IF @UserId IS NULL
BEGIN
    RAISERROR('No account with that email. Register in the app first.', 16, 1);
    RETURN;
END

IF @RoleId IS NULL
BEGIN
    RAISERROR('That role is not seeded.', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
    INSERT INTO UserRoles (UserId, RoleId, AssignedAtUtc) VALUES (@UserId, @RoleId, SYSUTCDATETIME());

SELECT '$Email now holds: ' + STRING_AGG(r.Name, ', ')
FROM UserRoles ur JOIN Roles r ON r.Id = ur.RoleId
WHERE ur.UserId = @UserId;
"@

docker exec $container /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P $password -C -d WhyStack -h -1 -W -Q $sql

Write-Host ''
Write-Host 'Sign out and sign in again in the app.' -ForegroundColor Yellow
Write-Host 'The role lives in the ACCESS TOKEN, and yours was minted before this ran — so the app is still'
Write-Host 'carrying the old one until it gets a new one. That is the token doing its job, not a bug.'
