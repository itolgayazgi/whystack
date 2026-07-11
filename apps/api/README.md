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

- .NET SDK 10.0

## Run

```bash
dotnet run --project src/WhyStack.Api
# or from the repo root:
pnpm api
```

## Test

```bash
dotnet test WhyStack.sln
# or from the repo root:
pnpm api:test
```

## Endpoints

| Endpoint | Purpose |
|---|---|
| `GET /health` | Liveness. Returns `200 Healthy`. |
| `GET /openapi/v1.json` | OpenAPI document. **Development only.** |
