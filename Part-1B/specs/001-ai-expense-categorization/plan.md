# Implementation Plan: AI Expense Categorization Assistant

**Branch**: `001-ai-expense-categorization` | **Date**: 2026-11-22 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-ai-expense-categorization/spec.md`

## Summary

A single-user assistant that parses free-text expense statements (via the Google Gemini API)
into structured expense records — amount, category, date, description — supporting multiple
expenses per message and requesting clarification when the amount cannot be determined. Model
output is never trusted directly: every response passes an independent three-layer evaluation
harness (structural/schema, invariant/property, semantic/quality) before being accepted.
Backend: .NET 10 / C# Web API calling Gemini's REST API directly (no persistence — this feature
is evaluated standalone, per spec Assumptions). Frontend: Angular 21 / TypeScript SPA.

## Technical Context

**Language/Version**: C# 13 on .NET 10 (backend); TypeScript 5.x on Angular 21 (frontend)

**Primary Dependencies**: ASP.NET Core Web API; `System.Net.Http.HttpClient` calling the Gemini
`generateContent` REST endpoint directly (no official Gemini .NET SDK needed for this scope);
Angular CLI + Angular HttpClient

**Storage**: None — this feature is evaluated standalone and does not persist extracted
expenses (spec Assumptions); the evaluation harness reads `evals/dataset.json` and writes
`evidence/eval-results.json`, both files, not a database.

**Testing**: xUnit for backend unit/contract tests, with the Gemini client abstracted behind an
interface so contract tests run against a fake/mocked response (no live API calls in CI-style
runs); a separate `.harness/` evaluation runner executes `evals/dataset.json` against the real
Gemini API for the non-deterministic evaluation pass.

**Target Platform**: Cross-platform .NET 10 web server (developed on Windows); Angular SPA
served as static assets

**Project Type**: Web application (backend API + frontend SPA), consistent with Part 1A

**Performance Goals**: Single-expense extraction completes in under 5 seconds end-to-end,
including external Gemini API latency (spec SC-005)

**Constraints**: Single-user only (no auth); genuinely non-deterministic AI component (contrast
with Part 1A); external API dependency (Gemini) whose failures must be distinguished from
application/evaluation failures in harness reporting (assignment Common Expectations)

**Scale/Scope**: Single user; evaluation dataset of ≥15 cases; no high-concurrency requirement

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Check | Status |
|---|---|---|
| I. Specification-First | Plan derived entirely from approved `spec.md`; no scope added here not traceable to a functional requirement | PASS |
| II. Behaviour Must Be Testable Despite Non-Determinism | Every requirement maps to a schema constraint, invariant, or explicit rubric — see `data-model.md` and `contracts/` | PASS |
| III. Validation at the Boundary | Harness validation is a separate service layer (`ValidationService`), called on every extraction response, independent of the Gemini client | PASS |
| IV. Harness Independence | Plan reserves `.harness/` for an evaluator independent of `src/`; `evals/dataset.json` is not touched by extraction code | PASS |
| V. Human Accountability | Plan does not automate the final ACCEPT/REJECT step; recorded manually in `evidence/harness-log.md` | PASS |

No violations. Complexity Tracking section below is intentionally empty.

## Project Structure

### Documentation (this feature)

```text
specs/001-ai-expense-categorization/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md         # Phase 1 output (/speckit-plan command)
├── contracts/            # Phase 1 output (/speckit-plan command)
└── tasks.md              # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root: `Part-1B/`)

```text
src/
├── backend/                       # ExpenseAssistant.Api (.NET 10)
│   ├── Models/                    # ExtractedExpense, ExtractionResponse DTOs
│   ├── Services/                  # GeminiClient, ExtractionService, ValidationService
│   ├── Controllers/                # REST endpoint
│   └── Config/                    # GeminiOptions (API key from env var/User Secrets)
├── frontend/                      # ExpenseAssistant.Web (Angular 21)
│   ├── components/                # Chat-style input, extraction result view
│   └── services/                  # HTTP client service calling the API
└── prompts/                        # System prompt + schema definition sent to Gemini

tests/
└── backend/
    ├── contract/                  # API contract tests (Gemini client mocked)
    └── unit/                      # Validation/invariant unit tests

evals/
└── dataset.json                   # ≥15 evaluation cases

.harness/
└── evaluator/                     # Structural + invariant + semantic evaluation runner

evidence/
├── initial-generation/
├── final-generation/
├── eval-results.json
└── harness-log.md
```

**Structure Decision**: Option 2 (web application: backend + frontend) from the template, plus
the assignment-mandated `evals/` and `.harness/` top-level directories for the non-deterministic
evaluation pipeline, matching the "Minimum Evidence Expected" structure in the assignment brief
(§4.9) at the `Part-1B/` root.

## Complexity Tracking

*No Constitution Check violations — this section intentionally left empty.*
