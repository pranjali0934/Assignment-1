<!--
Sync Impact Report
- Version change: [TEMPLATE] → 1.0.0 (initial ratification)
- Modified principles: n/a (first draft, all five principles newly defined)
- Added sections: Core Principles (5), Engineering Constraints, Quality Gates & Non-Negotiables, Governance
- Removed sections: none
- Follow-up TODOs: none
-->

# Personal Expense Tracker Constitution

## Core Principles

### I. Specification-First
No implementation work proceeds without an approved specification. If an ambiguity is
discovered during implementation, it MUST be resolved by updating the upstream Spec Kit
artifact (specification, clarification, plan, or tasks) — never by an ad-hoc code decision
that isn't reflected back in the spec.

### II. Deterministic Correctness
This application's behaviour MUST be fully deterministic. Every acceptance criterion MUST be
expressible as a binary (pass/fail) test. Vague, subjective, or "looks right" criteria are not
acceptable. Rationale: Part 1A is explicitly scoped as the deterministic counterpart to Part 1B,
so correctness must be mechanically verifiable, not judged.

### III. Validation at the Boundary
All business-rule validation — amount format, amount range, date constraints, category
membership — MUST be enforced at the API layer. The UI MAY also validate for user experience,
but MUST NOT be the only enforcement point. Rationale: client-side-only validation is trivially
bypassed and would make the harness's contract tests meaningless.

### IV. Harness Independence (NON-NEGOTIABLE)
Automated harness checks (lint, security/SAST, unit/contract tests) are independent of the
implementation and MUST NOT be modified, weakened, skipped, or disabled by the coding agent in
order to force a PASS. A failing check MUST be resolved by fixing the implementation, or by a
documented, approved specification change — never by editing the check itself to stop it from
catching the problem.

### V. Human Accountability
Every automated harness PASS MUST be followed by an explicit human ACCEPT/REJECT decision before
the result is considered final. The human reviewer remains responsible for the submitted software
regardless of what the harness reports.

## Engineering Constraints

- **Currency**: INR only. No multi-currency support.
- **Amount type**: Integer only. Decimal/fractional amounts MUST be rejected — never silently
  rounded or truncated.
- **Amount range**: Minimum ₹1, maximum ₹1,00,000, inclusive. Values outside this range MUST be
  rejected with a clear error.
- **Zero and negative amounts**: Always rejected.
- **Expense dates**: MUST NOT be future-dated relative to server time at submission. Future-dated
  expenses are rejected.
- **Categories**: MUST be selected from a fixed, predefined category list. Free-text categories
  are not permitted, so that filtering and summaries remain deterministic.
- **Persistence**: All CRUD operations MUST be atomic and consistent — a failed update must not
  partially apply.
- **Budget alerts**: Overspending alerts MUST trigger deterministically from defined budget-limit
  comparisons, never from fuzzy or approximate thresholds.
- **Technology stack**: Backend — .NET 10, C#, REST API. Frontend — Angular 21, TypeScript.
  Testing — xUnit for backend unit/contract tests.

## Quality Gates & Non-Negotiables

All of the following MUST pass before a harness run is considered a PASS:
- Static lint/style check (e.g. `dotnet format` / Roslyn analyzers for C#, ESLint for Angular) —
  zero errors.
- Static security (SAST) scan — no high/critical findings.
- xUnit contract tests derived directly from the Engineering Constraints above — 100% pass
  required, with no skipped or disabled tests.

The coding agent MUST NOT:
- Weaken, remove, or disable any acceptance criterion, test, lint rule, or security check to
  achieve a PASS.
- Introduce silent rounding, truncation, or coercion of invalid amounts.
- Commit secrets, API keys, or credentials.
- Report or record a harness result as PASS without it having actually run and passed.

## Governance

This constitution supersedes any conflicting ad-hoc practice for this project. Any amendment
MUST be made through this file or the corresponding feature specification, with the change and
its rationale recorded in the Sync Impact Report at the time of amendment. Versioning follows
semantic versioning: MAJOR for backward-incompatible principle removals/redefinitions, MINOR for
new principles or materially expanded guidance, PATCH for wording/clarification fixes. Every
harness run and human review MUST verify compliance with this constitution before a feature is
accepted.

**Version**: 1.0.0 | **Ratified**: 2026-11-13 | **Last Amended**: 2026-11-13
