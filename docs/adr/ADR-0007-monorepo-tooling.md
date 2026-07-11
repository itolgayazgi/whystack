# ADR-0007 — Monorepo Tooling

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Sources: Confirmed decision (2026-07-11); `06-monorepo-structure.md`, ADR-0001

---

## Context

`06-monorepo-structure.md` defines the folder layout but not the tooling that manages JavaScript/TypeScript packages, orchestrates builds/tests, or handles the coexistence of the React Native client with the .NET backend. ADR-0001 also collapses the Web and Mobile clients into a single React Native application, so the monorepo must host one client app, one API, and shared packages.

## Decision

1. **JS/TS dependency management:** **pnpm workspaces** (root `pnpm-workspace.yaml`), covering `packages/*` and the React Native client app.
2. **Task and cache orchestration:** **Turborepo** (`turbo.json`) for `build`, `lint`, `test`, `typecheck`, `content-validate` pipelines across workspaces.
3. **.NET backend:** `apps/api` remains a standard .NET solution managed by its own `.sln` / `Directory.Build.props` and `dotnet` CLI. It is invoked from the Turborepo pipeline as an external task where useful (e.g. CI orchestration) but is **not** a pnpm workspace.
4. **Node version** is pinned (`.nvmrc` / `packageManager` field) for reproducibility.
5. **Single React Native client app** (per ADR-0001) targets Web (RN-Web), Android and iOS. The `06` structure's separate `apps/web/` + `apps/mobile/` is consolidated (Phase 4 patch to `06`).
6. **Metro + pnpm caveat:** RN's Metro bundler and pnpm's symlinked `node_modules` require explicit configuration (Metro `watchFolders` / symlink resolution, or `node-linker=hoisted` for the client workspace). This configuration is a documented Sprint 1 setup task.

## Alternatives Considered

- **Nx.** Rejected for MVP: powerful (generators, project graph) but heavier setup and learning curve; premature for a solo-founder MVP.
- **Plain npm/yarn workspaces (no Turborepo).** Rejected: lacks task caching/orchestration; build coordination becomes manual as the repo grows.

## Consequences

- Root gains `pnpm-workspace.yaml`, `turbo.json`, pinned Node version.
- `06-monorepo-structure.md` is patched: single client app; tooling section added; `scripts/*` reference pnpm/Turborepo tasks.
- CI (`.github/workflows/`) runs Turborepo pipelines for JS/TS and `dotnet` for the API.
- Metro/pnpm interop is an explicit setup step, not an afterthought.

## References

- ADR-0001 (Web Platform Strategy)
- `06`
