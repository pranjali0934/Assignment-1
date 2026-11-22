# Phase 1 Data Model: AI Expense Categorization Assistant

## Entity: ExtractedExpense

A single parsed spending record (spec Key Entity, FR-002–FR-004).

| Field | Type | Validation Rules |
|---|---|---|
| `Amount` | integer | Required. Positive integer, 1–100,000 inclusive. Enforced by the harness independently of the model (Constitution Principle III) — never trusted from model output alone. |
| `Category` | string (enum) | Required. Must be one of the fixed category list: Food, Transport, Utilities, Entertainment, Other. If the model emits a category outside this set, the harness MUST map it to "Other" rather than reject the whole response (spec Edge Cases). |
| `Date` | date | Defaults to the request date if the model cannot determine one (spec Clarifications: date never blocks on clarification). |
| `Description` | string | Optional. Free text carried through from the model's interpretation. |

## Entity: ExtractionResponse

The full assistant output for one user message (spec Key Entity, FR-001, FR-006, FR-009).

| Field | Type | Validation Rules |
|---|---|---|
| `Expenses` | list of ExtractedExpense | Zero or more. Empty when `ClarificationNeeded` is true or when no expense is identifiable in the input (FR-009). |
| `ClarificationNeeded` | boolean | True whenever amount cannot be determined for one or more described expenses (FR-006). |
| `ClarificationQuestion` | string? | Required (non-null) when `ClarificationNeeded` is true; null otherwise. |

## Entity: EvaluationCase (`evals/dataset.json`)

One entry in the evaluation dataset (assignment §4.5).

| Field | Type | Purpose |
|---|---|---|
| `id` | string | Stable identifier for the case, used in `eval-results.json` traceability. |
| `category` | string | One of: `normal`, `boundary`, `ambiguous`, `missing-info`, `malformed`, `adversarial` — matches the assignment's required dataset coverage categories. |
| `input` | string | The free-text user message submitted to the assistant. |
| `expected` | object | Ground truth / expected properties (see below) — not a single exact string, since model wording varies (Constitution Principle II). |

### `expected` object shape (per case — fields present depend on the case)

| Field | Type | Meaning |
|---|---|---|
| `expectedExpenseCount` | integer? | Exact number of expenses the response should contain. |
| `expectedExpenses` | list of partial ExtractedExpense? | For each expected expense, the fields that MUST match exactly (e.g. `amount`, `category`); fields omitted here are not checked for exact match (e.g. free-text `description`, which the semantic layer scores instead). |
| `expectClarification` | boolean? | Whether `ClarificationNeeded` MUST be true. |
| `invariantChecks` | list of strings? | Named invariant assertions this case specifically targets (e.g. `"amount-in-range"`, `"category-is-enum-value"`, `"no-fabrication"`) — used to report which Layer-2 checks a case exercises. |

## Entity: EvaluationResult (`evidence/eval-results.json`, harness output)

| Field | Type | Meaning |
|---|---|---|
| `caseId` | string | Matches `EvaluationCase.id`. |
| `structuralPass` | boolean | Layer 1 result. |
| `invariantPass` | boolean | Layer 2 result. |
| `semanticScore` | number (0–1) | Layer 3 result against the case's rubric. |
| `overallPass` | boolean | True only if all three layers pass their respective thresholds for this case. |
| `rawModelOutput` | string | The model's raw response, preserved for evidence/debugging. |
| `failureReason` | string? | Populated when `overallPass` is false, naming which layer/check failed. |

## Derived view: Aggregate Evaluation Summary

Not a stored entity — computed after running all cases (spec SC-001):

- **Pass rate** = count of cases where `overallPass` is true ÷ total cases.
- **Aggregate PASS** = pass rate ≥ 85% (constitution threshold).
