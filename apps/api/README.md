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

## Endpoints

| Endpoint | Purpose | Touches the database? |
|---|---|---|
| `GET /health` | **Liveness** — is the process alive? | **No, deliberately.** A database outage must not make an orchestrator restart every instance; restarting the API does not fix SQL Server. |
| `GET /health/ready` | **Readiness** — can this instance serve a request? | **Yes.** Returns `503` when SQL Server is unreachable, which means "stop sending me traffic", and recovers on its own. |
| `GET /openapi/v1.json` | OpenAPI document | Development only. |
