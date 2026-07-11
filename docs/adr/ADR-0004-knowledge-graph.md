# ADR-0004 — Knowledge Graph

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Sources: Decision 06; `10-content-architecture.md`, `05-system-architecture.md`, `07-database-design.md`, `08-api-standards.md`

---

## Context

The Knowledge Graph was described in `00`, `01`, `05` and `10`. Ownership was unclear, and topic relationships risked being modeled both as graph edges and as free-text sections (see ADR-0002).

## Decision

1. **`10-content-architecture.md` owns the Knowledge Graph definition** (concepts, relationship types, graph-quality rules). It is the Single Source of Truth. No other document redefines the graph.
2. **Layer responsibilities:**
   - **Database (`07`)** *stores* graph relationships (`TopicRelationships`, roadmap prerequisites).
   - **API (`08`)** *exposes* relationships (e.g. `GET /topics/{id}/relationships`).
   - **Search (ADR/Decision 07)** *consumes* relationships (Knowledge Graph Expansion step).
   - **AI (`11`/`14`)** *consumes* relationships for grounding context.
3. **Relationship types** are the canonical list in `10` (e.g. `Requires, Next, Related, Alternative, Uses, UsedBy, Improves, Affects, ReplacedBy, DeprecatedBy`). `07`'s `TopicRelationships.RelationshipType` must be a superset-compatible projection of this list.
4. **Prerequisites / Related Topics / Next Topics are graph edges** rendered as topic sections (ADR-0002), not independent stored bodies.
5. **MVP storage is SQL Server.** A dedicated graph database is **not** part of the MVP; it remains a future option only if measured relationship-traversal needs justify it (preserved from `05`/`07`/`10`).

## Alternatives Considered

- **Graph database for MVP.** Rejected: no measured need; SQL relationship tables are sufficient and simpler (MVP-first).
- **Relationships defined per consuming document.** Rejected: causes divergence; violates SSoT.

## Consequences

- `00`, `01`, `05` Knowledge Graph passages become references to `10` (Phase 4 patches).
- Relationship-type list is reconciled between `07` and `10` (Phase 4 patch).
- Search's "Knowledge Graph Expansion" step (Decision 07) has a defined, owned source.

## References

- Decision 06, Decision 07
- ADR-0002 (Canonical Topic Model)
- `05`, `07`, `08`, `10`
