# Phase 1 Data Model: Personal Expense Tracker

## Entity: Expense

A single recorded spending event (spec Key Entity, FR-001–FR-006).

| Field | Type | Validation Rules |
|---|---|---|
| `Id` | integer (PK) | System-generated, immutable |
| `Amount` | integer | Required. Positive integer, 1–100,000 inclusive. Reject decimal input at the API boundary (do not round/truncate). Reject 0 and negative values. |
| `Category` | string (enum) | Required. Must be one of the fixed category list (see Category entity). |
| `Date` | date | Required. Must be ≤ current server date (future dates rejected). |
| `Description` | string | Optional. Free text. |
| `CreatedAt` / `UpdatedAt` | timestamp | System-managed, for audit only — not user-editable. |

**Relationships**: Many `Expense` records reference one `Category` (by value, not a separate
table row, since categories are fixed). `Expense` records are implicitly grouped by
(`Category`, year-month of `Date`) when computing summaries and budget comparisons.

**State transitions**: Created → Edited (any number of times, same validation re-applied) →
Deleted (hard delete, per spec Assumptions). No other states.

## Entity: Category

A fixed, predefined classification value (spec Key Entity, FR-006).

| Field | Type | Notes |
|---|---|---|
| `Name` | string | One of: `Food`, `Transport`, `Utilities`, `Entertainment`, `Other` (fixed set; not user-editable in this scope, per spec Assumptions). |

**Relationships**: Referenced by `Expense.Category` and `BudgetLimit.Category`.

## Entity: BudgetLimit

A per-category, per-month spending ceiling (spec Key Entity, FR-010–FR-011).

| Field | Type | Validation Rules |
|---|---|---|
| `Id` | integer (PK) | System-generated |
| `Category` | string (enum) | Required. Must be one of the fixed category list. |
| `Year` | integer | Required. |
| `Month` | integer (1–12) | Required. |
| `LimitAmount` | integer | Required. Positive integer, 1–100,000 inclusive (same amount rules as `Expense.Amount`). |

**Relationships**: Unique per (`Category`, `Year`, `Month`) — setting a new limit for the same
category/month replaces the previous value. Compared against the sum of `Expense.Amount` for
matching `Category` and year-month to determine overspending (FR-011).

## Derived view: Monthly Summary

Not a stored entity — computed on request (FR-009, FR-012):

- **Total spent** for a given month = sum of `Expense.Amount` where `Date` falls in that month.
- **Per-category breakdown** = same sum, grouped by `Category`.
- **Overspending alert** = raised per (`Category`, month) where per-category total > the matching
  `BudgetLimit.LimitAmount`, if one is set for that category/month (edge case: no alert if no
  limit is set, per spec Edge Cases).

Recomputed on every read (no cached/stale summary state), satisfying FR-012's consistency
requirement.
