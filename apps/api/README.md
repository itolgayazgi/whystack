# WhyStack API

ASP.NET Core backend for WhyStack. Modular monolith, four layers.

Structure and layer rules are owned by `docs/sprint-0/06-monorepo-structure.md`,
`docs/sprint-0/05-system-architecture.md` and `docs/sprint-0/12-engineering-standards.md`.
This file only tells you how to run it.

## Layers

| Project | Responsibility | May depend on |
|---|---|---|
| `WhyStack.Domain` | Rules and invariants | **Nothing** |
| `WhyStack.Application` | Use cases, ports (contracts) | Domain |
| `WhyStack.Infrastructure` | EF Core, SQL Server, AI providers, storage — implements Application contracts | Application |
| `WhyStack.Api` | HTTP surface + composition root: bind → validate → authorize → call use case → map response | Application, Infrastructure |

The dependency direction is enforced by project references. `Api` references `Infrastructure`
**only** to register implementations in DI — no infrastructure types may leak into endpoints.

## Prerequisites

- .NET SDK 10 (pinned by `global.json`)
- Docker, for the local SQL Server

## Setup — once

```powershell
./scripts/setup/dev-database.ps1
```

That one script starts SQL Server, **generates** the SA password, writes the connection string to
**user secrets**, and applies the migrations. The password is never written to a tracked file.

Add `-Reset` to destroy the local database and start again.

The API refuses to start without a connection string, by design: an API that boots with no database
and only fails when a user touches it is worse than one that does not boot.

## Run

```bash
dotnet run --project src/WhyStack.Api    # or, from the repo root: pnpm api
```

## Test

```bash
dotnet test WhyStack.sln                 # or, from the repo root: pnpm api:test
```

The API tests are **integration tests** — they drive the real HTTP pipeline and the real database.
SQL Server must be running.

## Migrations

Conventions, the destructive-change checklist, and what CI does and does not verify:
**`docs/engineering/database-migrations.md`**.

```bash
dotnet tool restore   # pins dotnet-ef to the version this repo expects
```

## Email

`dev-database.ps1` starts **Mailpit** alongside SQL Server. It catches every message and delivers
none of them — read them at **<http://localhost:8025>**.

This is how the confirmation and password-reset flows are actually exercised: register, open the mail,
click the link. Pointing development at a real provider would mean sending test email to real inboxes,
and would make the flow untestable without an account and an API key.

**SMTP, not a provider SDK.** Resend, SendGrid, Postmark, Mailgun and SES all speak SMTP, so the
provider is a deployment decision (`Smtp:Host`, `Smtp:Username`, `Smtp:Password`) rather than a code
one.

**An environment with no `Smtp:Host` refuses to start** — outside Development. An API that cannot send
a confirmation email must not accept registrations and silently deliver nothing.

## Endpoints

| Endpoint | Purpose | Touches the database? |
|---|---|---|
| `GET /health` | **Liveness** — is the process alive? | **No, deliberately.** A database outage must not make an orchestrator restart every instance; restarting the API does not fix SQL Server. |
| `GET /health/ready` | **Readiness** — can this instance serve a request? | **Yes.** Returns `503` when SQL Server is unreachable, which means "stop sending me traffic", and recovers on its own. |
| `GET /openapi/v1.json` | OpenAPI document | Development only. |

### Authentication

| Endpoint | Note |
|---|---|
| `POST /api/v1/auth/register` | Answers **identically** whether the address was taken. The inbox owner is told the truth. |
| `POST /api/v1/auth/login` | Web gets an HttpOnly cookie; native gets the refresh token in the body. Never both. |
| `POST /api/v1/auth/refresh` | Single-use. Replaying a rotated token **revokes the whole session family** — it means the token leaked. |
| `POST /api/v1/auth/logout` | Succeeds even when there is nothing to end. |
| `POST /api/v1/auth/confirm-email` | Single-use, 24 hours. Confirms the address the token was **sent to**, not whatever the account says now. |
| `POST /api/v1/auth/resend-confirmation` | Same answer for unknown, unconfirmed and already-confirmed addresses. |
| `POST /api/v1/auth/forgot-password` | Same answer whether or not the address has an account. |
| `POST /api/v1/auth/reset-password` | Single-use, **1 hour**. **Signs out every device** — a reset that leaves an attacker's session alive resets nothing. |
