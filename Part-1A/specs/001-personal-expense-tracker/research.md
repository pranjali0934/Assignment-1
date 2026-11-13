# Phase 0 Research: Personal Expense Tracker

No `[NEEDS CLARIFICATION]` markers remained in the Technical Context — the technology stack was
fixed in advance (.NET 10 / C# / Angular 21 / TypeScript / xUnit). This document records the
remaining implementation decisions and their rationale.

## Decision: Persistence — SQL Server via EF Core

**Decision**: Use SQL Server as the storage engine, accessed through Entity Framework Core
(project requirement — fixed by the team, not chosen from alternatives below).

**Rationale**: The spec requires date-range filtering, category filtering, and per-category
monthly aggregation (summaries, budget comparison) — all naturally expressed as SQL queries. SQL
Server gives transactional guarantees for atomic CRUD operations (Constitution Engineering
Constraint: "Persistence: all CRUD operations must be atomic and consistent"), and EF Core gives
strongly-typed, testable query code and migrations for the `Expense`/`BudgetLimit` schema
regardless of the underlying relational provider. Local development uses a local SQL Server
instance (e.g. SQL Server Express/Developer edition or a SQL Server container); test runs use a
dedicated test database created/dropped per test run (or respawned per test) rather than a
production instance.

**Alternatives considered**:
- *SQLite (single local file)*: Simplest to set up for a single-user assignment, no server
  process required. Not used — team specified SQL Server explicitly.
- *Flat JSON file persistence*: Would require hand-rolled in-memory filtering/grouping logic for
  date-range and per-category aggregation, and weaker atomicity guarantees than a relational
  engine. Rejected.
- *In-memory only (no persistence)*: Fails FR-013 (data must survive restarts). Rejected.

## Decision: Testing approach — xUnit + `WebApplicationFactory`

**Decision**: Use xUnit for all backend tests. Contract/integration tests use
`Microsoft.AspNetCore.Mvc.Testing`'s `WebApplicationFactory` against an in-memory/SQLite test
database so API behaviour (status codes, validation errors, computed summaries) is verified
end-to-end without a real network call.

**Rationale**: `WebApplicationFactory` lets contract tests assert on the actual HTTP contract
(request/response shape, status codes for rejected amounts/dates) rather than just internal
service logic — directly satisfying the harness's "Unit & Contract Tests" gate.

**Alternatives considered**: Testing only at the service-class level (no HTTP layer). Rejected
because it would not catch boundary-enforcement regressions at the API layer, which Constitution
Principle III explicitly requires to be checked.

## Decision: Category list representation

**Decision**: The fixed category list (Food, Transport, Utilities, Entertainment, Other) is
defined as a server-side enum/constant set, validated on every create/edit request; the frontend
reads the same list from a `GET /api/categories` endpoint rather than hard-coding it twice.

**Rationale**: Keeps the single source of truth on the backend (Constitution Principle III:
validation at the boundary) while avoiding drift between frontend and backend category lists.

**Alternatives considered**: Hard-coding the category list independently in the Angular frontend.
Rejected — risks the frontend allowing a category the backend would reject, producing a
confusing user-facing error instead of a disabled/absent option.

## Output

All Technical Context items are resolved; no outstanding `[NEEDS CLARIFICATION]` markers remain.
Proceeding to Phase 1 (data model, contracts, quickstart).
