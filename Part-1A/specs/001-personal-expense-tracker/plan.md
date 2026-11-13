# Implementation Plan: Personal Expense Tracker

**Branch**: `001-personal-expense-tracker` | **Date**: 2026-11-13 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-personal-expense-tracker/spec.md`

## Summary

A single-user expense tracking web application. Users record, edit and delete expenses (amount,
category, date, description) through a REST API; a fixed category list and strict amount/date
validation (positive integer ₹1–₹1,00,000, no future dates) are enforced server-side. Users filter
expenses by date range and/or category, view monthly per-category spending summaries, and set
per-category monthly budget limits that trigger overspending alerts. Backend: .NET 10 / C# Web API
with EF Core over SQL Server. Frontend: Angular 21 / TypeScript SPA consuming the API.

## Technical Context

**Language/Version**: C# 13 on .NET 10 (backend); TypeScript 5.x on Angular 21 (frontend)

**Primary Dependencies**: ASP.NET Core Web API, Entity Framework Core (SQL Server provider),
Angular CLI + Angular HttpClient/Reactive Forms

**Storage**: SQL Server, via EF Core — see `research.md` for rationale vs. SQLite/JSON file
persistence

**Testing**: xUnit + `WebApplicationFactory` for backend unit/contract/integration tests; Angular
Karma/Jasmine for frontend unit tests

**Target Platform**: Cross-platform .NET 10 web server (developed on Windows); Angular SPA served
as static assets

**Project Type**: Web application (backend API + frontend SPA)

**Performance Goals**: Single-user, low-volume workload — all CRUD and summary operations
respond in under 2 seconds (per spec SC-001); no high-concurrency requirement

**Constraints**: Single-user only (no auth/multi-tenancy); fully deterministic — no AI/ML
component in this part; offline capability not required

**Scale/Scope**: Single user; expected data volume in the hundreds to low thousands of expense
records — no special indexing or partitioning needed at this scale

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design._

| Principle                       | Check                                                                                                                                                             | Status |
| ------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| I. Specification-First          | Plan derived entirely from approved `spec.md`; no scope added here that isn't traceable to a functional requirement                                               | PASS   |
| II. Deterministic Correctness   | No AI/ML or non-deterministic component in this part; all behaviour is rule-based and unit-testable                                                               | PASS   |
| III. Validation at the Boundary | All amount/date/category validation is designed into the backend API layer (not the Angular UI alone) — see `data-model.md` validation rules and `contracts/`     | PASS   |
| IV. Harness Independence        | Plan reserves `.harness/` for lint, SAST and xUnit contract tests independent of application code; no design decision couples harness to implementation internals | PASS   |
| V. Human Accountability         | Plan does not automate the final ACCEPT/REJECT step; that remains a manual review recorded in `evidence/harness-log.md`                                           | PASS   |

No violations. Complexity Tracking section below is intentionally empty.

## Project Structure

### Documentation (this feature)

```text
specs/001-personal-expense-tracker/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root: `Part-1A/`)

```text
src/
├── backend/                       # ExpenseTracker.Api (.NET 10)
│   ├── Models/                    # Expense, Category, BudgetLimit
│   ├── Services/                  # Validation, summary/budget calculation
│   ├── Controllers/               # REST endpoints
│   └── Data/                      # EF Core DbContext, SQL Server configuration
└── frontend/                      # ExpenseTracker.Web (Angular 21)
    ├── components/                # Expense list/form, filters, summary view
    ├── pages/
    └── services/                  # HTTP client services calling the API

tests/
├── backend/
│   ├── contract/                  # API contract tests (xUnit)
│   └── unit/                      # Validation/service unit tests (xUnit)
└── frontend/                      # Angular component/service unit tests
```

**Structure Decision**: Option 2 (web application: backend + frontend) from the template, using
the directory names above under `Part-1A/`. This matches the assignment's required minimum
evidence structure (`src/`, `tests/`, `.harness/`, `evidence/` at the `Part-1A/` root), with
`src/` and `tests/` split into `backend/`/`frontend/` subfolders for the two stacks.

## Complexity Tracking

_No Constitution Check violations — this section intentionally left empty._
