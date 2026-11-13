# Assignment 1 — Spec-Driven Development & Harness Engineering

**Course:** AI-Augmented SDLC (SE/ZG534)
**Mode:** Individual
**Total Marks:** 25 (Part 1A: 10, Part 1B: 10, Part 2 — Reflection: 5)

Two small applications built using the GitHub Spec Kit spec-driven development (SDD) workflow, each validated by an independent harness that checks AI-generated code for correctness, security and spec compliance — not just that it runs.

## Repository Structure

```
Assignment-1/
├── README.md
├── Part-1A/                  # Deterministic application
│   ├── specs/                # Spec Kit artifacts (constitution, spec, plan, tasks)
│   ├── src/                  # Application source
│   ├── tests/                # xUnit contract/unit tests
│   ├── .harness/             # Lint, SAST, test-runner scripts
│   └── evidence/             # Initial/final generations, harness results, harness-log.md
└── Part-1B/                  # Non-deterministic AI application
    ├── specs/                # Spec Kit artifacts
    ├── src/                  # Application source
    ├── prompts/              # Prompt/system-instruction definitions
    ├── tests/
    ├── evals/
    │   └── dataset.json       # ≥15 evaluation cases
    ├── .harness/              # Schema/invariant/semantic evaluators + runner
    └── evidence/               # Initial/final generations, eval-results.json, harness-log.md
```

## Part 1A — Personal Expense Tracker

A deterministic CRUD application for tracking personal expenses.

**Features**
- Add, edit and delete expenses
- Categorize expenses
- Monthly spending summaries
- Budget limits and overspending alerts
- Filter by date and category

**Validation rules (amounts)**

| Rule | Decision |
|---|---|
| Currency | INR only |
| Amount format | Integer only |
| Minimum amount | ₹1 |
| Maximum amount | ₹1,00,000 |
| Decimal values | Reject |
| Zero | Reject |
| Negative values | Reject |
| Future-dated expenses | Reject |

**Harness checks:** calculation correctness, CRUD behaviour, invalid-amount rejection, date-range handling, budget-rule enforcement.

## Part 1B — AI Expense Categorization Assistant

A non-deterministic AI feature that parses free-text expense statements into structured records.

**Example**

Input: `"I spent 450 rupees on dinner yesterday."`

Extracted:
- Amount: 450
- Category: Food
- Date: interpreted date
- Description: Dinner

The assistant asks for clarification when mandatory information (amount or date) is missing, and supports multiple expenses in a single sentence, e.g.:

Input: `"I spent ₹500 on groceries and ₹200 on petrol."`

```json
{
  "expenses": [
    { "amount": 500, "category": "Food" },
    { "amount": 200, "category": "Transport" }
  ]
}
```

**Extraction rules:** amounts must be positive integers between 1 and 100,000 inclusive; decimal amounts are rejected (never silently rounded); zero and negative amounts are rejected.

**Harness checks:** output schema validity, correct amount extraction, category classification, date interpretation, clarification behaviour — evaluated via a layered pipeline (structural → invariant/property → semantic/quality) against a declared pass threshold.

## Technology Stack

- Backend: .NET 10, C#
- Frontend: Angular 21, TypeScript
- Testing: xUnit
- SDD workflow: GitHub Spec Kit (`/speckit.constitution`, `/speckit.specify`, `/speckit.clarify`, `/speckit.plan`, `/speckit.tasks`, `/speckit.analyze`, `/speckit.implement`)

## Workflow

Each part follows the same SDD → harness cycle:

```
Problem statement → constitution → specify → clarify → plan → tasks → analyze → implement
        → Harness validation → feedback/repair → revalidation → human ACCEPT/REJECT decision
```

All Spec Kit artifacts, generation history, harness results and the final human decision are preserved under each part's `evidence/` directory and summarized in `harness-log.md`.

## Deliverables

1. This GitHub repository (public), with complete source, specs, harness and evidence for both parts.
2. A short demonstration video (shared via Drive/OneDrive).
3. A Reflection Report (submitted separately to LMS).

## Secrets

No API keys, credentials or `.env` files are committed to this repository.
