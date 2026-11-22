---
description: "Task list for Personal Expense Tracker implementation"
---

# Tasks: Personal Expense Tracker

**Input**: Design documents from `/specs/001-personal-expense-tracker/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md, quickstart.md

**Tests**: Included and mandatory. The project constitution (Principle IV — Harness
Independence) and the assignment brief require deterministic contract/unit tests derived from
the specification as a harness gate, not an optional extra.

**Organization**: Tasks are grouped by user story (from spec.md) to enable independent
implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- File paths are relative to `Part-1A/`

## Path Conventions (from plan.md)

- Backend: `src/backend/` (ExpenseTracker.Api, .NET 10)
- Frontend: `src/frontend/` (ExpenseTracker.Web, Angular 21)
- Backend tests: `tests/backend/contract/`, `tests/backend/unit/`
- Frontend tests: `tests/frontend/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create solution structure per plan.md: `src/backend/`, `src/frontend/`,
      `tests/backend/contract/`, `tests/backend/unit/`, `tests/frontend/`
- [X] T002 Initialize .NET 10 Web API project `ExpenseTracker.Api` in `src/backend/`, adding
      `Microsoft.EntityFrameworkCore.SqlServer` and `Microsoft.EntityFrameworkCore.Design` packages
- [X] T003 [P] Initialize xUnit test project `ExpenseTracker.Api.Tests` referencing
      `ExpenseTracker.Api` in `tests/backend/`, with `Microsoft.AspNetCore.Mvc.Testing` for
      `WebApplicationFactory`-based contract tests
- [X] T004 [P] Initialize Angular 21 app `ExpenseTracker.Web` via Angular CLI in `src/frontend/`
- [X] T005 [P] Configure backend lint/style: enable Roslyn analyzers / `dotnet format` config in
      `src/backend/.editorconfig`
- [X] T006 [P] Configure frontend lint: ESLint config for `src/frontend/`

**Checkpoint**: Projects build and run empty; test projects execute with zero tests.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before any user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T007 Define the fixed category list (`Food`, `Transport`, `Utilities`, `Entertainment`,
      `Other`) as a C# enum/constant set in `src/backend/Models/Category.cs` (data-model.md:
      Category entity)
- [X] T008 Create `ExpenseTrackerDbContext` (EF Core, SQL Server provider, Windows
      Authentication connection string) in `src/backend/Data/ExpenseTrackerDbContext.cs`
- [X] T009 Configure the SQL Server connection string in `src/backend/appsettings.json` (Windows
      Authentication — no credentials to protect, per project decision)
- [X] T010 [P] Set up API error-handling middleware producing the contract's validation error
      shape `{ "errors": { "<field>": ["<message>"] } }` in `src/backend/Middleware/`
      (contracts/api.md)
- [X] T011 [P] Set up a single Angular `environment.ts` with the backend API base URL in
      `src/frontend/src/environments/` (no dev/prod split needed — this runs in one local
      environment only)
- [X] T012 Implement `GET /api/categories` returning the fixed category list in
      `src/backend/Controllers/CategoriesController.cs` (depends on T007) — this is the single
      source of truth the frontend reads from (research.md decision)

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 1 - Record and manage expenses (Priority: P1) 🎯 MVP

**Goal**: Users can create, edit and delete expenses, with amount/date validation enforced
server-side (spec.md User Story 1; FR-001–FR-005).

**Independent Test**: Create an expense via the API, confirm it's retrievable, edit a field,
confirm the change persisted, delete it, confirm it's gone — and confirm invalid
amount/date submissions are rejected with `400`.

### Tests for User Story 1

> Write these tests FIRST; confirm they FAIL before implementation.

- [X] T013 [P] [US1] Contract test: `POST /api/expenses` with a valid body returns `201 Created`
      with the created expense in under 2 seconds (SC-001), in
      `tests/backend/contract/ExpenseCreateTests.cs`
- [X] T014 [P] [US1] Contract test: `POST /api/expenses` rejects amount `500.50` (decimal), `0`,
      and `-200` each with `400 Bad Request` — verbatim rule from data-model.md: "Positive integer,
      1–100,000 inclusive... Reject 0 and negative values", in
      `tests/backend/contract/ExpenseAmountValidationTests.cs`
- [X] T015 [P] [US1] Contract test: `POST /api/expenses` with a future `date` returns
      `400 Bad Request`, in `tests/backend/contract/ExpenseDateValidationTests.cs`
- [X] T016 [P] [US1] Contract test: `PUT /api/expenses/{id}` edits an existing expense and
      returns `200 OK` with updated fields; editing a non-existent `id` returns `404`, in
      `tests/backend/contract/ExpenseEditTests.cs`
- [X] T017 [P] [US1] Contract test: `DELETE /api/expenses/{id}` returns `204 No Content` and the
      expense no longer appears in `GET /api/expenses`; deleting a non-existent `id` returns `404`,
      in `tests/backend/contract/ExpenseDeleteTests.cs`

### Implementation for User Story 1

- [X] T018 [P] [US1] Create `Expense` model in `src/backend/Models/Expense.cs` with fields
      `Amount` (int), `Category` (enum), `Date` (date), `Description` (string, optional),
      `CreatedAt`/`UpdatedAt` (timestamp, system-managed) — per data-model.md
- [X] T019 [US1] Add `Expense` `DbSet` to `ExpenseTrackerDbContext` and create/apply the initial
      EF Core migration in `src/backend/Data/Migrations/` (depends on T018)
- [X] T020 [US1] Implement `ExpenseValidationService` in
      `src/backend/Services/ExpenseValidationService.cs` enforcing: amount is a positive integer in
      [1, 100000] inclusive (reject decimal/zero/negative, never round/truncate); date ≤ current
      server date; category is one of the fixed list — verbatim rules from the constitution and
      data-model.md
- [X] T021 [US1] Implement `ExpenseService` create/edit/delete methods in
      `src/backend/Services/ExpenseService.cs`, calling `ExpenseValidationService` before any write
      (depends on T019, T020)
- [X] T022 [US1] Implement `ExpensesController` with `POST /api/expenses`,
      `PUT /api/expenses/{id}`, `DELETE /api/expenses/{id}` in
      `src/backend/Controllers/ExpensesController.cs`, returning the exact status codes from
      contracts/api.md (depends on T021)
- [X] T023 [P] [US1] Implement Angular `ExpenseService` (HTTP client wrapper for create/edit/
      delete) in `src/frontend/src/app/services/expense.service.ts`
- [X] T024 [US1] Implement Angular expense form component (create/edit) with client-side
      pre-validation mirroring the backend rules (UX only — backend remains the enforcement point
      per Constitution Principle III) in `src/frontend/src/app/components/expense-form/`
- [X] T025 [US1] Implement Angular expense list component with edit/delete actions in
      `src/frontend/src/app/components/expense-list/` (depends on T023)

**Checkpoint**: User Story 1 is fully functional and independently testable (CRUD + validation).

---

## Phase 4: User Story 2 - Categorize and filter expenses (Priority: P2)

**Goal**: Users filter their expense list by category and/or date range, and category is
enforced against the fixed list (spec.md User Story 2; FR-006–FR-008).

**Independent Test**: Create expenses across ≥2 categories and dates; filter by category alone,
by date range alone, and by both combined; confirm only matching expenses are returned, and an
invalid category or an inverted date range (`from` after `to`) is rejected with `400`.

### Tests for User Story 2

- [X] T026 [P] [US2] Contract test: `POST /api/expenses` with a category outside the fixed list
      returns `400 Bad Request`, in `tests/backend/contract/ExpenseCategoryValidationTests.cs`
- [X] T027 [P] [US2] Contract test: `GET /api/expenses?category=Food` returns only `Food`
      expenses, in `tests/backend/contract/ExpenseFilterCategoryTests.cs`
- [X] T028 [P] [US2] Contract test: `GET /api/expenses?from=&to=` returns only expenses with
      `Date` inside the inclusive range, in `tests/backend/contract/ExpenseFilterDateRangeTests.cs`
- [X] T029 [P] [US2] Contract test: `GET /api/expenses?from=&to=` with `from` after `to` returns
      `400 Bad Request` (FR-008), in `tests/backend/contract/ExpenseFilterInvalidRangeTests.cs`

### Implementation for User Story 2

- [X] T030 [US2] Extend `ExpenseService` with category and inclusive date-range filtering query
      logic in `src/backend/Services/ExpenseService.cs` (depends on T021)
- [X] T031 [US2] Add `GET /api/expenses` with `category`, `from`, `to` query parameters to
      `ExpensesController`, validating `from ≤ to` before querying, in
      `src/backend/Controllers/ExpensesController.cs` (depends on T030)
- [X] T032 [P] [US2] Implement Angular filter component (category dropdown populated from
      `GET /api/categories`, date-range picker) in
      `src/frontend/src/app/components/expense-filter/`
- [X] T033 [US2] Wire the filter component's selections into `ExpenseService` list queries and
      the expense list view in `src/frontend/src/app/components/expense-list/` (depends on T023,
      T032)

**Checkpoint**: User Stories 1 and 2 both work independently.

---

## Phase 5: User Story 3 - Monthly summaries and budget alerts (Priority: P3)

**Goal**: Users view a monthly per-category spending summary and set budget limits that trigger
overspending alerts (spec.md User Story 3; FR-009–FR-012).

**Independent Test**: Create expenses totalling a known amount in one category within one month;
set a budget limit below that total; confirm the summary shows the correct total and an
overspending alert; confirm no alert is produced when no limit is set or spending is within
limit.

### Tests for User Story 3

- [X] T034 [P] [US3] Contract test: `GET /api/summary?year=&month=` returns the correct total
      and per-category breakdown for a month with known expenses, in
      `tests/backend/contract/SummaryTotalsTests.cs`
- [X] T035 [P] [US3] Contract test: `POST /api/budgets` sets a limit and returns `200 OK`;
      posting again for the same category/year/month replaces the previous value, in
      `tests/backend/contract/BudgetSetTests.cs`
- [X] T036 [P] [US3] Contract test: `GET /api/summary` reports `overBudget: true` for a
      category/month where total spending exceeds its budget limit, and `overBudget: false` when at
      or below it, in `tests/backend/contract/SummaryOverBudgetTests.cs`
- [X] T037 [P] [US3] Contract test: `GET /api/summary` reports `budgetLimit: null`,
      `overBudget: false` for a category/month with no budget limit set (spec.md Edge Cases), in
      `tests/backend/contract/SummaryNoBudgetSetTests.cs`

### Implementation for User Story 3

- [X] T038 [P] [US3] Create `BudgetLimit` model in `src/backend/Models/BudgetLimit.cs` with
      fields `Category`, `Year`, `Month`, `LimitAmount` (same amount rule as `Expense.Amount`),
      unique per `(Category, Year, Month)` — per data-model.md
- [X] T039 [US3] Add `BudgetLimit` `DbSet` + unique index/constraint and migration in
      `src/backend/Data/ExpenseTrackerDbContext.cs` / `src/backend/Data/Migrations/` (depends on
      T038)
- [X] T040 [US3] Implement `BudgetService` (set/replace, get-by-month) in
      `src/backend/Services/BudgetService.cs`, validating `LimitAmount` via
      `ExpenseValidationService`'s amount rule (depends on T039, T020)
- [X] T041 [US3] Implement `SummaryService` computing monthly total, per-category breakdown, and
      `overBudget` comparison against `BudgetService` in
      `src/backend/Services/SummaryService.cs` (depends on T030, T040)
- [X] T042 [US3] Implement `BudgetsController` (`POST /api/budgets`, `GET /api/budgets`) in
      `src/backend/Controllers/BudgetsController.cs` (depends on T040)
- [X] T043 [US3] Implement `SummaryController` (`GET /api/summary`) in
      `src/backend/Controllers/SummaryController.cs` (depends on T041)
- [X] T044 [P] [US3] Implement Angular summary view component (total + per-category breakdown +
      overspending indicator) in `src/frontend/src/app/components/summary-view/`
- [X] T045 [P] [US3] Implement Angular budget-limit form component in
      `src/frontend/src/app/components/budget-form/`

**Checkpoint**: All three user stories are independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Harness readiness and quality gates that span all user stories

### Frontend Unit Tests (previously missing — added after cross-artifact analysis)

- [X] T046 [P] [US1] Angular unit test for the expense form component (create/edit, client-side
      pre-validation display) in `tests/frontend/expense-form.component.spec.ts`
- [X] T047 [P] [US1] Angular unit test for the expense list component (edit/delete action
      wiring) in `tests/frontend/expense-list.component.spec.ts`
- [X] T048 [P] [US1] Angular unit test for `ExpenseService` (HTTP calls for create/edit/delete)
      in `tests/frontend/expense.service.spec.ts`
- [X] T049 [P] [US2] Angular unit test for the expense filter component (category/date-range
      selection emits correct query params) in `tests/frontend/expense-filter.component.spec.ts`
- [X] T050 [P] [US3] Angular unit test for the summary view component (renders totals,
      per-category breakdown, overspending indicator) in
      `tests/frontend/summary-view.component.spec.ts`
- [X] T051 [P] [US3] Angular unit test for the budget form component (submission and validation
      display) in `tests/frontend/budget-form.component.spec.ts`

### Backend Unit Tests & Harness Gates

- [X] T052 [P] Unit tests for `ExpenseValidationService` boundary cases (₹1 and ₹1,00,000
      accepted; ₹0, negative, decimal, and future-dated rejected) in
      `tests/backend/unit/ExpenseValidationServiceTests.cs`
- [X] T053 [P] Unit tests for `SummaryService` aggregation and `overBudget` logic (including the
      "no budget set" edge case) in `tests/backend/unit/SummaryServiceTests.cs`
- [X] T054 Run `dotnet format` / Roslyn analyzers across `src/backend/` and resolve all findings
      (Constitution Quality Gate)
- [X] T055 Run ESLint across `src/frontend/` and resolve all findings (Constitution Quality Gate)
- [X] T056 Run a SAST scan across `src/backend/` (SecurityCodeScan.VS2019 Roslyn analyzer —
      Semgrep does not support native Windows) and `src/frontend/` (eslint-plugin-security) and
      resolve any high/critical findings (Constitution Quality Gate). Zero findings in either.
- [X] T057 Execute the quickstart.md end-to-end validation scenarios manually and record the
      outcome
- [X] T058 Record initial-generation output, harness run results, and the human ACCEPT/REJECT
      decision in `evidence/` and `evidence/harness-log.md` (assignment §3.6–3.7)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately.
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories.
- **User Stories (Phase 3–5)**: All depend on Foundational completion.
  - US1 (P1) has no dependency on US2/US3.
  - US2 (P2) extends `ExpenseService`/`ExpensesController` from US1 (T021–T023) — built after
    US1, but its filtering behaviour is independently testable from US1's CRUD behaviour.
  - US3 (P3) depends on `ExpenseService` (US1, for summary totals) but its budget/summary logic
    is independently testable.
- **Polish (Phase 6)**: Depends on all three user stories being complete.

### Within Each User Story

- Tests are written first and MUST fail before implementation begins.
- Models before services; services before controllers; backend before the frontend components
  that consume it.

### Parallel Opportunities

- All Setup tasks marked [P] (T003–T006) can run in parallel.
- All contract tests within a story's "Tests" section (marked [P]) can run in parallel — they
  touch different test files.
- T018 (Expense model) and T007 (Category enum, Phase 2) don't conflict and can be parallelized
  across phases once Phase 2's blocking tasks are otherwise done.

---

## Parallel Example: User Story 1

```text
# Tests for User Story 1 (run together):
Task: "Contract test POST /api/expenses valid → 201 in tests/backend/contract/ExpenseCreateTests.cs"
Task: "Contract test POST /api/expenses invalid amount → 400 in tests/backend/contract/ExpenseAmountValidationTests.cs"
Task: "Contract test POST /api/expenses future date → 400 in tests/backend/contract/ExpenseDateValidationTests.cs"
Task: "Contract test PUT /api/expenses/{id} → 200 in tests/backend/contract/ExpenseEditTests.cs"
Task: "Contract test DELETE /api/expenses/{id} → 204 in tests/backend/contract/ExpenseDeleteTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 (Setup) and Phase 2 (Foundational — blocks everything).
2. Complete Phase 3 (User Story 1 — CRUD + validation).
3. **STOP and VALIDATE**: run T013–T017 contract tests; confirm all pass independently of US2/US3.
4. This is the assignment's minimum viable, demonstrable slice.

### Incremental Delivery

1. Setup + Foundational → foundation ready.
2. Add User Story 1 → validate independently (MVP).
3. Add User Story 2 → validate independently (filtering doesn't break US1's CRUD tests).
4. Add User Story 3 → validate independently (summaries/budgets don't break US1/US2 tests).
5. Phase 6 (Polish) → run full harness (lint + SAST + all xUnit tests) before the human
   ACCEPT/REJECT decision.

## Notes

- [P] tasks touch different files with no unmet dependencies.
- [Story] labels give traceability back to spec.md's prioritized user stories.
- Every validation rule quoted in a task description is verbatim from data-model.md/constitution,
  so it isn't left to implementation-time discretion (Constitution Principle I).
- Commit after each task or logical group, so the harness-log.md's evidence trail is traceable
  to specific commits.
