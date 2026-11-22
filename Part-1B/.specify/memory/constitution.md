<!--
Sync Impact Report
- Version change: [TEMPLATE] → 1.0.0 (initial ratification)
- Modified principles: n/a (first draft, all five principles newly defined)
- Added sections: Core Principles (5), Engineering Constraints, Quality Gates & Non-Negotiables, Governance
- Removed sections: none
- Follow-up TODOs: none
-->

# AI Expense Categorization Assistant Constitution

## Core Principles

### I. Specification-First
No implementation work proceeds without an approved specification. If an ambiguity is
discovered during implementation, it MUST be resolved by updating the upstream Spec Kit
artifact (specification, clarification, plan, or tasks) — never by an ad-hoc code decision
that isn't reflected back in the spec.

### II. Behaviour Must Be Testable Despite Non-Determinism
Model wording MAY vary, but every requirement MUST be reducible to a machine-checkable
property: a schema constraint, an invariant/property check, or an explicit semantic/quality
rubric. "The AI should understand the request" is not an acceptable requirement; "the output
MUST contain a numeric `amount` field that is a positive integer in [1, 100000]" is.

### III. Validation at the Boundary (Independent of the Model)
Every model output MUST pass structural, invariant, and guardrail checks before it is trusted
or persisted — regardless of what the model itself claims. The evaluation harness is
independent of the extraction prompt/implementation and MUST NOT be weakened to make a
non-conforming output pass.

### IV. Harness Independence (NON-NEGOTIABLE)
The evaluation harness (schema validation, invariant/property checks, semantic/quality
scoring) MUST NOT be modified, weakened, skipped, or disabled to force a PASS. A failing case
MUST be resolved by improving the prompt, parser, validator, or guardrail — or by a documented,
approved specification change — never by editing the evaluation itself to stop catching the
problem. Benchmark cases MUST NOT be removed because they are difficult.

### V. Human Accountability
Every automated evaluation PASS MUST be followed by an explicit human ACCEPT/REJECT decision
before the result is considered final. The human reviewer remains responsible for the
submitted software and evidence regardless of what the harness reports.

## Engineering Constraints

- **LLM provider**: Google Gemini API (`generativelanguage.googleapis.com`), called via a
  thin HTTP client. The API key MUST be supplied via environment variable
  (`GEMINI_API_KEY`) or .NET User Secrets — never hardcoded or committed.
- **Output schema contract**: The model MUST return `{ "expenses": [ { "amount": <integer>,
  "category": <string>, "date": <string|null>, "description": <string|null> } ],
  "clarificationNeeded": <boolean>, "clarificationQuestion": <string|null> }`. Fields that are
  present MUST conform to these types regardless of input phrasing.
- **Amount type**: Integer only. Decimal/fractional amounts in the extracted result MUST be
  rejected by the harness, never silently rounded or truncated.
- **Amount range**: Minimum ₹1, maximum ₹1,00,000, inclusive (same rule as Part 1A, for a
  consistent definition of a valid expense across the assignment).
- **Category list**: The same fixed category list as Part 1A — Food, Transport, Utilities,
  Entertainment, Other — so extracted categories are always one of these, never free text.
- **Multi-expense input**: A single user message MAY describe multiple expenses (e.g. "₹500 on
  groceries and ₹200 on petrol"); the harness MUST verify each extracted expense independently
  against the above rules.
- **Clarification behaviour**: When mandatory information (amount or date) cannot be
  determined from the input, the model's response MUST set `clarificationNeeded: true` with a
  specific `clarificationQuestion`, rather than guessing or fabricating a value.
- **Technology stack**: Backend — .NET 10, C#, REST API. Frontend — Angular 21, TypeScript.
  Testing — xUnit for backend unit/contract tests; `evals/dataset.json` with ≥15 cases for the
  non-deterministic evaluation harness.

## Quality Gates & Non-Negotiables

A harness run is a PASS only when all of the following hold:
- **Layer 1 — Structural validation**: output is valid JSON conforming to the schema contract
  above (required fields present, correct types, enum-valid category values).
- **Layer 2 — Property/invariant validation**: amount in [1, 100000] and integer for every
  extracted expense; clarification is requested whenever amount or date is genuinely
  undeterminable from the input; no expense is fabricated when the input describes none.
- **Layer 3 — Semantic/quality evaluation**: category and date are evaluated against
  ground-truth/expected values from `evals/dataset.json` using an explicit rubric (exact match
  where defined, acceptable-alternatives list for ambiguous cases) — never a bare LLM-as-Judge
  call standing in for Layers 1–2.
- **Aggregate threshold**: at least 85% of evaluation cases MUST pass all three layers for the
  harness to report an overall PASS. (Rationale: a handful of genuinely ambiguous/adversarial
  cases are expected to be hard; this is not a 100%-or-nothing gate, consistent with
  real-world LLM evaluation practice, while still catching systemic failures.)
- **Static checks**: backend lint (Roslyn analyzers / `dotnet format`) and SAST (consistent
  with Part 1A's tool choices) MUST also pass with zero findings.

The coding agent MUST NOT:
- Remove or weaken a benchmark case in `evals/dataset.json` to raise the pass rate.
- Lower the 85% threshold without a documented, approved specification change.
- Disable a guardrail (e.g. amount-range validation) to make a case pass.
- Report or record an evaluation result as PASS without it having actually run.
- Commit the Gemini API key or any other secret.

## Governance

This constitution supersedes any conflicting ad-hoc practice for this project. Any amendment
MUST be made through this file or the corresponding feature specification, with the change and
its rationale recorded in the Sync Impact Report at the time of amendment. Versioning follows
semantic versioning: MAJOR for backward-incompatible principle removals/redefinitions, MINOR
for new principles or materially expanded guidance, PATCH for wording/clarification fixes.
Every harness run and human review MUST verify compliance with this constitution before a
feature is accepted.

**Version**: 1.0.0 | **Ratified**: 2026-11-22 | **Last Amended**: 2026-11-22
