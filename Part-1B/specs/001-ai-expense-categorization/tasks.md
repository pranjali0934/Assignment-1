---
description: "Task list for AI Expense Categorization Assistant implementation"
---

# Tasks: AI Expense Categorization Assistant

**Input**: Design documents from `/specs/001-ai-expense-categorization/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md, quickstart.md

**Tests**: Included and mandatory, per the constitution (Principle IV — Harness Independence)
and the assignment brief's requirement for a structural + invariant + semantic evaluation
pipeline independent of the generated implementation.

**Organization**: Tasks are grouped by user story (from spec.md), plus a dedicated Evaluation
Harness phase (assignment §4.5–4.7) that is cross-cutting infrastructure required to actually
assess all three stories' non-deterministic behaviour.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- File paths are relative to `Part-1B/`

## Path Conventions (from plan.md)

- Backend: `src/backend/` (ExpenseAssistant.Api, .NET 10)
- Frontend: `src/frontend/` (ExpenseAssistant.Web, Angular 21)
- Prompts: `src/prompts/`
- Backend tests: `tests/backend/contract/`, `tests/backend/unit/`
- Evaluation dataset: `evals/dataset.json`
- Evaluation harness: `.harness/evaluator/`

---

## Phase 1: Setup (Shared Infrastructure)

- [X] T001 Create solution structure per plan.md: `src/backend/`, `src/frontend/`,
      `src/prompts/`, `tests/backend/contract/`, `tests/backend/unit/`, `evals/`,
      `.harness/evaluator/`, `evidence/initial-generation/`, `evidence/final-generation/`
- [X] T002 Initialize .NET 10 Web API project `ExpenseAssistant.Api` in `src/backend/`
- [X] T003 [P] Initialize xUnit test project `ExpenseAssistant.Api.Tests` referencing
      `ExpenseAssistant.Api` in `tests/backend/`, with `Microsoft.AspNetCore.Mvc.Testing`
- [X] T004 [P] Initialize Angular 21 app `ExpenseAssistant.Web` via Angular CLI in
      `src/frontend/`
- [X] T005 [P] Configure backend lint/style: `.editorconfig` in `src/backend/` (reuse Part 1A's
      settings for consistency) plus SecurityCodeScan.VS2019 package reference
- [X] T006 [P] Configure frontend lint: ESLint (`angular-eslint` + `eslint-plugin-security` flat
      config) for `src/frontend/`

**Checkpoint**: Projects build and run empty; test projects execute with zero tests.

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T007 Define `ExtractedExpense`, `ExtractionResponse`, and a `Category` enum (same fixed
      five values as Part 1A: Food, Transport, Utilities, Entertainment, Other) in
      `src/backend/Models/`, per data-model.md
- [X] T008 Define `GeminiOptions` (API key from `GEMINI_API_KEY` env var or User Secrets, model
      name, configurable) in `src/backend/Config/GeminiOptions.cs`
- [X] T009 Write the system prompt + Gemini `responseSchema` definition (forcing the
      `ExtractionResponse` JSON shape via controlled generation) in
      `src/prompts/extraction-prompt.md`
- [X] T010 Define `IGeminiClient` interface and implement `GeminiClient` (HttpClient call to
      `generateContent` using the prompt/schema from T009) in
      `src/backend/Services/GeminiClient.cs` — per research.md's "direct REST, no SDK" decision
- [X] T011 Implement `ValidationService` enforcing: amount integer in [1, 100000] independent of
      model output; category mapped to the fixed enum (unknown → "Other"); no fabricated
      expense when the input provides none — in `src/backend/Services/ValidationService.cs`
- [X] T012 Implement deterministic date defaulting in `ValidationService`/`ExtractionService`:
      when the model's response omits a date or returns one that fails to parse, set `Date` to
      the current request date in code — never relying on the Gemini prompt alone to supply
      this default (closes the Constitution Principle III gap for the one field, per
      cross-artifact analysis, that previously had no independent enforcement) — in
      `src/backend/Services/ValidationService.cs` (depends on T011)
- [X] T013 [P] Implement error-handling middleware distinguishing a Gemini API/service failure
      (`502 Bad Gateway`) from an empty/invalid request (`400`) in
      `src/backend/Middleware/ExtractionErrorMiddleware.cs` — per contracts/api.md
- [X] T014 [P] Set up a single Angular `environment.ts` with the backend API base URL in
      `src/frontend/src/environments/`
- [X] T015 Implement `ExtractionService` orchestrating `GeminiClient` → `ValidationService`,
      with one retry-then-fallback-to-clarification on structurally invalid model output (per
      contracts/api.md) in `src/backend/Services/ExtractionService.cs` (depends on T010, T011)
- [X] T016 Implement `ExtractController` (`POST /api/extract`) in
      `src/backend/Controllers/ExtractController.cs` (depends on T015)

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 1 - Extract a single well-formed expense (Priority: P1) 🎯 MVP

**Goal**: A plain-language single-expense sentence returns one correctly structured expense
(spec.md User Story 1; FR-001–FR-004).

**Independent Test**: Submit a single clear sentence and confirm the response contains exactly
one expense with correct amount, valid category, and interpreted date.

### Tests for User Story 1 ⚠️

> Write these tests FIRST; confirm they FAIL before implementation.

- [X] T017 [P] [US1] Implement a fake `IGeminiClient` test double returning canned JSON
      responses (valid, decimal-amount, malformed) for use by all contract tests, in
      `tests/backend/FakeGeminiClient.cs`
- [X] T018 [P] [US1] Contract test: `POST /api/extract` with a message the fake client resolves
      to one valid expense returns `200 OK` with `amount`, `category`, `date` matching — in
      `tests/backend/contract/ExtractSingleExpenseTests.cs`
- [X] T019 [P] [US1] Contract test: `POST /api/extract` where the fake client's raw response
      contains a decimal amount (e.g. 500.50) results in the harness rejecting that amount — not
      silently rounding it — in `tests/backend/contract/ExtractDecimalAmountTests.cs`
- [X] T020 [P] [US1] Contract test: `POST /api/extract` with an empty `message` returns
      `400 Bad Request`, in `tests/backend/contract/ExtractEmptyMessageTests.cs`

### Implementation for User Story 1

- [X] T021 [P] [US1] Implement Angular chat-input component (free-text message box, submit) in
      `src/frontend/src/app/components/chat-input/`
- [X] T022 [P] [US1] Implement Angular extraction-result component (renders expense(s) or
      clarification question) in `src/frontend/src/app/components/extraction-result/`
- [X] T023 [US1] Implement Angular `ExtractionService` (HTTP client wrapper for
      `POST /api/extract`) in `src/frontend/src/app/services/extraction.service.ts` (depends on
      T021, T022)

**Checkpoint**: User Story 1 is fully functional and independently testable.

---

## Phase 4: User Story 2 - Extract multiple expenses from one message (Priority: P2)

**Goal**: A message describing ≥2 expenses returns one structured entry per expense (spec.md
User Story 2; FR-005).

**Independent Test**: Submit a sentence describing two expenses in different categories;
confirm both are returned as separate, correctly attributed entries.

### Tests for User Story 2 ⚠️

- [X] T024 [P] [US2] Contract test: `POST /api/extract` where the fake client resolves to two
      expenses returns both, correctly attributed (amount/category each), in
      `tests/backend/contract/ExtractMultipleExpensesTests.cs`
- [X] T025 [P] [US2] Contract test: `POST /api/extract` where one of two described expenses is
      well-formed and the other is missing its amount returns the well-formed expense AND
      `clarificationNeeded: true` for the incomplete one (partial success, per spec Edge
      Cases), in `tests/backend/contract/ExtractPartialMultipleExpensesTests.cs`

### Implementation for User Story 2

- [X] T026 [US2] Extend `ValidationService`/`ExtractionService` to validate each expense in a
      multi-expense response independently (depends on T011, T015)
- [X] T027 [US2] Update the Angular extraction-result component to render a list of expenses in
      `src/frontend/src/app/components/extraction-result/` (depends on T022)

**Checkpoint**: User Stories 1 and 2 both work independently.

---

## Phase 5: User Story 3 - Request clarification when mandatory information is missing (Priority: P3)

**Goal**: Input with no determinable amount triggers a specific clarification question, with no
fabricated expense record (spec.md User Story 3; FR-006, FR-009, FR-010).

**Independent Test**: Submit a message with no amount; confirm `clarificationNeeded: true` with
a specific question, and an empty `expenses` array.

### Tests for User Story 3 ⚠️

- [X] T028 [P] [US3] Contract test: `POST /api/extract` where the fake client resolves to "no
      amount determinable" returns `clarificationNeeded: true`, a non-empty
      `clarificationQuestion`, and empty `expenses`, in
      `tests/backend/contract/ExtractClarificationTests.cs`
- [X] T029 [P] [US3] Contract test: `POST /api/extract` with gibberish input (fake client
      resolves to "no expense identifiable") returns `clarificationNeeded: true` with no
      fabricated expense, in `tests/backend/contract/ExtractGibberishTests.cs`
- [X] T030 [P] [US3] Contract test: `POST /api/extract` with an adversarial/prompt-injection
      message whose fake response claims an out-of-range amount (e.g. 999999) is still rejected
      by `ValidationService` regardless of the model's claim, in
      `tests/backend/contract/ExtractAdversarialAmountTests.cs`

### Implementation for User Story 3

- [X] T031 [US3] Implement the no-fabrication guard in `ValidationService`: when amount cannot
      be determined for an expense, that expense is dropped and `ClarificationNeeded` is set,
      never a guessed amount (depends on T011)
- [X] T032 [US3] Add clarification-question display state to the chat-input/extraction-result
      components in `src/frontend/src/app/components/` (depends on T021, T022)

**Checkpoint**: All three user stories are independently functional.

---

## Phase 6: Evaluation Dataset & Harness (assignment §4.5–4.7)

**Purpose**: The single non-deterministic evaluation pipeline that actually assesses US1–US3
against real Gemini output — cross-cutting, required before any harness PASS/FAIL is possible.

- [X] T033 Create `evals/dataset.json` with ≥15 cases, per data-model.md's `EvaluationCase`
      shape, covering: normal/representative inputs, boundary amounts (₹1, ₹1,00,000),
      ambiguous/underspecified inputs, missing mandatory information, malformed inputs, and
      adversarial/guardrail cases (assignment §4.5) — at least one case per category
- [X] T034 [P] Implement the Layer 1 structural validator (JSON schema conformance against
      `ExtractionResponse`) in `.harness/evaluator/StructuralValidator.cs`
- [X] T035 [P] Implement the Layer 2 invariant validator (amount range, category-enum mapping,
      clarification-vs-fabrication correctness — reusing `ValidationService`'s rules) in
      `.harness/evaluator/InvariantValidator.cs`
- [X] T036 [P] Implement the Layer 3 semantic/quality evaluator (exact-match scoring against
      `expected.expectedExpenses`/`expectClarification`, with an explicit rubric for ambiguous
      cases) in `.harness/evaluator/SemanticEvaluator.cs`
- [X] T037 Implement the evaluator entry point: calls the real `ExtractionService` (live Gemini
      API) for each case, runs all three layers, computes `overallPass` per case, and writes
      `evidence/eval-results.json` (depends on T033–T036), in `.harness/evaluator/Program.cs`
- [X] T038 Implement aggregate scoring (pass rate ≥ 85% per constitution) and distinguish
      Gemini API/service failures from evaluation failures in the evaluator's output (depends
      on T037)
- [X] T039 Record per-case response latency in the evaluator (T037) and assert SC-005 (single
      extraction completes in under 5 seconds) against the live Gemini API; report average and
      max latency alongside the pass rate in `evidence/eval-results.json` (depends on T037,
      T038) — closes the gap identified in cross-artifact analysis, where SC-005 previously had
      no task measuring it (contract tests use a fake client and are near-instant, so this can
      only be measured against the real API here)

**Checkpoint**: Harness can run end-to-end against the live API and produce a PASS/FAIL verdict.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [X] T040 [P] Unit tests for `ValidationService` boundary cases (₹1 and ₹1,00,000 accepted;
      ₹0, negative, decimal rejected; unknown category mapped to "Other") in
      `tests/backend/unit/ValidationServiceTests.cs`
- [X] T041 [P] Unit tests for the no-fabrication guard (amount-undeterminable → clarification,
      never a guessed value) in `tests/backend/unit/NoFabricationGuardTests.cs`
- [X] T042 Run `dotnet format` / Roslyn analyzers across `src/backend/` and resolve all findings
- [X] T043 Run ESLint across `src/frontend/` and resolve all findings
- [X] T044 Run SAST (SecurityCodeScan backend, eslint-plugin-security frontend — consistent
      with Part 1A's tool choices; Semgrep is not used for the same Windows-compatibility
      reason documented in Part 1A) and resolve any high/critical findings
- [X] T045 Execute the quickstart.md end-to-end validation scenarios manually against the live
      Gemini API and record the outcome
- [X] T046 Run the full evaluation harness (T037/T038) against all `evals/dataset.json` cases
      with the live Gemini API; record `evidence/eval-results.json`
- [X] T047 Record initial-generation output, harness run results (including at least one
      genuine failure-and-repair cycle if encountered), and the final human ACCEPT/REJECT
      decision in `evidence/` and `evidence/harness-log.md` (assignment §4.9–4.10)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately.
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories.
- **User Stories (Phase 3–5)**: All depend on Foundational completion.
  - US1 (P1) has no dependency on US2/US3.
  - US2 (P2) extends `ValidationService`/`ExtractionService` from Foundational + the UI
    component from US1 — built after US1, but independently testable via its own contract tests.
  - US3 (P3) extends `ValidationService` similarly; independently testable.
- **Evaluation Harness (Phase 6)**: Depends on Foundational (needs `ExtractionResponse` shape
  and `ExtractionService`) but not on US1–US3's UI work — can start once Phase 2 is done,
  in parallel with Phases 3–5.
- **Polish (Phase 7)**: Depends on all of Phases 3–6 being complete.

### Within Each User Story

- Tests are written first and MUST fail before implementation begins.
- Models/config before services; services before controllers; backend before frontend.

### Parallel Opportunities

- All Setup tasks marked [P] (T003–T006) can run in parallel.
- All contract tests within a story's "Tests" section (marked [P]) can run in parallel.
- Phase 6 (Evaluation Harness) can proceed in parallel with Phases 3–5 once Phase 2 is done,
  since it depends only on the foundational `ExtractionResponse` shape and `ExtractionService`,
  not on the Angular UI components.

---

## Parallel Example: User Story 1

```text
# Tests for User Story 1 (run together):
Task: "Contract test valid single expense in tests/backend/contract/ExtractSingleExpenseTests.cs"
Task: "Contract test decimal amount handling in tests/backend/contract/ExtractDecimalAmountTests.cs"
Task: "Contract test empty message → 400 in tests/backend/contract/ExtractEmptyMessageTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 (Setup) and Phase 2 (Foundational — blocks everything).
2. Complete Phase 3 (User Story 1 — single-expense extraction).
3. **STOP and VALIDATE**: run T018–T020 contract tests; confirm all pass independently of US2/US3.
4. This is the assignment's minimum viable, demonstrable slice.

### Incremental Delivery

1. Setup + Foundational → foundation ready.
2. Add User Story 1 → validate independently (MVP).
3. Add User Story 2 → validate independently.
4. Add User Story 3 → validate independently.
5. Build the Evaluation Harness (Phase 6) — can overlap with Phases 3–5 since it only needs the
   Foundational layer.
6. Phase 7 (Polish) → run full harness (lint + SAST + unit/contract tests + live evaluation run)
   before the human ACCEPT/REJECT decision.

## Notes

- [P] tasks touch different files with no unmet dependencies.
- [Story] labels give traceability back to spec.md's prioritized user stories.
- Every validation rule quoted in a task description is verbatim from data-model.md/constitution.
- Commit after each task or logical group, so `harness-log.md`'s evidence trail is traceable to
  specific commits.
- Contract tests (Phase 3–5) never call the live Gemini API — they use the fake client (T017)
  so `dotnet test` is fast, deterministic, and runnable without a `GEMINI_API_KEY`. Only the
  Evaluation Harness (Phase 6) and the manual quickstart validation (T045) call the real API.
