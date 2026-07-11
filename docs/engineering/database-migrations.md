# Database Migrations — Conventions

- Status: Active
- Owner: WhyStack Core Team
- Policy owner: **`07-database-design.md`** — that document owns *what* the rules are.
  This one owns *how* you follow them.

---

## Where things live

| Thing | Location | Why |
|---|---|---|
| `WhyStackDbContext` | `apps/api/src/WhyStack.Infrastructure/Persistence/` | EF Core is an Infrastructure concern. It may not appear in Application or in an endpoint. |
| Migrations | `.../Persistence/Migrations/` | Same assembly as the context. |
| Entity configuration | `.../Persistence/Configurations/` | One `IEntityTypeConfiguration<T>` per entity, discovered automatically. `OnModelCreating` stays empty. |
| Connection string | **user secrets**, never a tracked file | It carries a password. A password in a tracked file is a password in the git history forever. |

---

## Naming

```
{Category}_{Description}
```

The category is one of the five in `07`. Prefixing with it means the **risk of a migration is visible
in `git log` and in the migrations list**, before anyone opens the file:

| Category | Means | Examples |
|---|---|---|
| `Schema_` | Structure change | `Schema_Initial`, `Schema_AddUsers` |
| `Data_` | Moves or rewrites existing rows | `Data_BackfillTopicSlugs` |
| `Seed_` | Inserts reference data | `Seed_Languages`, `Seed_Roles` |
| `Index_` | Index only | `Index_TopicsSlugUnique` |
| `Cleanup_` | **Destructive.** Drops or narrows something | `Cleanup_DropLegacyColumn` |

A reviewer who sees `Cleanup_` knows to open the destructive checklist below. A reviewer who sees
`Schema_` knows they can read it quickly. That is the entire point.

---

## Adding one

```bash
cd apps/api
dotnet tool restore                      # pins dotnet-ef to the version this repo expects

dotnet dotnet-ef migrations add Schema_AddUsers \
  --project src/WhyStack.Infrastructure \
  --startup-project src/WhyStack.Api \
  --output-dir Persistence/Migrations
```

Then **read the generated SQL before you trust it**:

```bash
dotnet dotnet-ef migrations script --idempotent \
  --project src/WhyStack.Infrastructure \
  --startup-project src/WhyStack.Api
```

EF's model diff is a guess about your intent. It is usually right. When it is wrong — a rename it
reads as a drop-plus-add, and your data is gone — it is wrong in a way that only the SQL shows.

Apply it locally:

```bash
dotnet dotnet-ef database update \
  --project src/WhyStack.Infrastructure \
  --startup-project src/WhyStack.Api
```

---

## Rules

**A migration that has been applied anywhere but your own machine is immutable.** Do not edit it, do
not renumber it, do not "just fix" it. Add a new one. Editing an applied migration means the
`__EFMigrationsHistory` row no longer describes what actually ran, and every other environment is now
silently out of sync.

**Never change the schema by hand in a deployed environment.** If it did not come from a migration,
the next migration will be diffing against a schema that no longer matches the model snapshot, and it
will generate nonsense.

**Every migration must run against an empty database.** CI enforces this on every pull request: it
starts a blank SQL Server and applies the whole chain from nothing. A migration that only works
against a database that already has last week's schema is a migration that fails on the first fresh
environment — which is usually production.

**One logical change per migration.** A migration that adds a table *and* backfills it *and* drops an
old column cannot be reasoned about, reviewed, or rolled back.

---

## Destructive migrations

`Cleanup_` — and any migration that drops a table or column, changes a column type, deletes data, or
renames without a compatibility step — requires all of the following before merge (`07`):

- [ ] Explicit approval, from a human, recorded on the pull request
- [ ] Backup taken and verified restorable
- [ ] Rollback plan written down — not "we would restore the backup", but the actual steps
- [ ] Data migration plan, if any data must survive
- [ ] Validated on staging against production-shaped data
- [ ] Production impact reviewed: table size, lock duration, whether it blocks writes

**A rename is a drop plus an add** unless you explicitly tell EF otherwise. That is the single most
common way a migration destroys data. Read the generated SQL.

---

## What CI checks

On every pull request, the `API` job:

1. Starts an empty SQL Server 2022 container.
2. Applies every migration from scratch.
3. Runs the integration tests against it, including the readiness health check.

It does **not** yet check: rollback (`Down`), migration performance against large tables, or seed data
validity. Those matter once there is data. They are not checked today, and this sentence exists so
that nobody assumes otherwise.

---

## Health checks

Two endpoints, and the distinction matters:

| Endpoint | Answers | Includes the database? |
|---|---|---|
| `GET /health` | "Is this process alive?" | **No.** |
| `GET /health/ready` | "Can this instance serve a request?" | **Yes.** |

Liveness deliberately ignores the database. If it did not, a SQL Server outage would make the
orchestrator kill and restart every API instance — which does not fix SQL Server, and turns a degraded
service into no service at all. Readiness failing means *"stop sending me traffic"*, which is the
correct response, and it recovers on its own when the database comes back.
