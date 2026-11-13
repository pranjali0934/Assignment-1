# Feature Specification: Personal Expense Tracker

**Feature Branch**: `001-personal-expense-tracker`

**Created**: 2026-11-13

**Status**: Draft

**Input**: User description: "Personal Expense Tracker — add, edit and delete expenses; categorize expenses; monthly spending summaries; budget limits and overspending alerts; filter by date and category. Amounts are INR, positive integers only, between 1 and 100000 inclusive; decimals, zero, negative and future-dated expenses are rejected."

## Clarifications

### Session 2026-11-13

- Q: What exact set of expense categories should the fixed category list contain? → A: Food, Transport, Utilities, Entertainment, Other.
- Q: Should a budget limit be settable for a future month, before any expenses exist for it? → A: Yes — budget limits may be set for any month (past, current, or future); only expense dates themselves are restricted from being future-dated.
- Q: Should a user be able to remove a budget limit entirely, not just replace it with a new value? → A: No — a budget limit can only be replaced with a new value; there is no delete/unset capability in this scope.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record and manage expenses (Priority: P1)

A user records a new expense (amount, category, date, description), and can later edit or delete
it if they made a mistake or the expense changed.

**Why this priority**: This is the foundational capability. Without reliable create/edit/delete,
no other feature (summaries, budgets, filters) has trustworthy data to work with.

**Independent Test**: Can be fully tested by creating an expense via the API, confirming it is
retrievable, editing one of its fields, confirming the change persisted, then deleting it and
confirming it no longer appears — all without needing categorization, summaries, or budgets.

**Acceptance Scenarios**:

1. **Given** no existing expenses, **When** a user adds an expense of ₹500 dated today in category
   "Food" with description "Lunch", **Then** the expense is created and appears in the expense list
   with those exact values.
2. **Given** an existing expense, **When** the user edits its amount to ₹750, **Then** the stored
   expense reflects ₹750 and all dependent views (summaries, budget totals) recompute from the new
   value.
3. **Given** an existing expense, **When** the user deletes it, **Then** it no longer appears in the
   expense list or in any summary/budget calculation.
4. **Given** a user attempts to add an expense with amount ₹500.50, zero, a negative value, or a
   future date, **When** the submission is made, **Then** the system rejects it with a clear
   validation error and no expense is created.

---

### User Story 2 - Categorize and filter expenses (Priority: P2)

A user assigns each expense to a category from a fixed list, and later filters their expense list
by date range and/or category to find specific expenses.

**Why this priority**: Categorization and filtering are what make a growing expense list usable;
they are needed before summaries/budgets are meaningful, but the app is still minimally usable
without them (User Story 1 alone).

**Independent Test**: Can be fully tested by creating expenses in at least two categories across
different dates, then filtering by a single category and by a date range, and confirming only the
matching expenses are returned.

**Acceptance Scenarios**:

1. **Given** expenses exist in categories "Food" and "Transport", **When** the user filters by
   "Food", **Then** only "Food" expenses are returned.
2. **Given** expenses exist across several dates, **When** the user filters by a date range,
   **Then** only expenses with a date inside that range (inclusive) are returned.
3. **Given** a user attempts to assign a category not in the predefined list, **When** the
   submission is made, **Then** the system rejects it with a validation error.

---

### User Story 3 - Monthly summaries and budget alerts (Priority: P3)

A user views a monthly spending summary (total and per-category breakdown) and sets a budget limit
per category per month; the system alerts them when spending in that category exceeds the limit.

**Why this priority**: This is the highest-value insight feature, but it depends on reliable
CRUD and categorization (Stories 1–2) already being in place to compute correct totals.

**Independent Test**: Can be fully tested by creating several expenses in one category within one
month, setting a budget limit for that category/month below the total spent, and confirming the
summary shows the correct total and an overspending alert is raised.

**Acceptance Scenarios**:

1. **Given** expenses of ₹300 and ₹400 in "Food" within the same month, **When** the user requests
   the monthly summary, **Then** the "Food" total for that month is ₹700.
2. **Given** a budget limit of ₹1,000 set for "Food" in a given month, **When** total "Food"
   spending for that month reaches ₹1,100, **Then** an overspending alert is produced referencing
   that category and month.
3. **Given** total spending in a category remains at or below its budget limit, **When** the
   summary is generated, **Then** no overspending alert is produced for that category.

---

### Edge Cases

- What happens when a user filters by a date range where `from` is after `to`? System MUST reject
  the filter request with a validation error rather than silently returning an empty result.
- What happens when an expense is edited to move it into a different month? Both the old and new
  month's summaries/budget calculations MUST reflect the change after the edit.
- What happens when no budget limit has been set for a category/month? The summary MUST still show
  the spending total; no overspending alert is produced since there is nothing to compare against.
- What happens when an expense amount exactly equals the minimum (₹1) or maximum (₹1,00,000)
  boundary? Both boundary values MUST be accepted (range is inclusive).
- What happens when an expense's date is exactly "today" (server date)? It MUST be accepted; only
  dates strictly after today are rejected as future-dated.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow a user to create an expense with amount, category, date, and an
  optional description.
- **FR-002**: System MUST allow a user to edit any field of an existing expense, subject to the
  same validation rules as creation.
- **FR-003**: System MUST allow a user to delete an existing expense.
- **FR-004**: System MUST reject any expense amount that is not a positive integer between 1 and
  100,000 (inclusive), including decimal, zero, and negative values, with a clear validation error.
- **FR-005**: System MUST reject any expense dated later than the current server date.
- **FR-006**: System MUST require every expense to have a category selected from a fixed,
  predefined category list, and MUST reject categories outside that list.
- **FR-007**: System MUST allow a user to filter expenses by category, by date range (inclusive),
  or both combined.
- **FR-008**: System MUST reject a date-range filter where the start date is after the end date.
- **FR-009**: System MUST compute a monthly spending summary showing the total spent and a
  per-category breakdown for a given month.
- **FR-010**: System MUST allow a user to set a budget limit for a given category and month,
  where the month may be past, current, or future (forward planning is permitted — unlike
  expense dates, budget-limit months are not restricted from being in the future).
- **FR-011**: System MUST raise an overspending alert for a category/month whenever total spending
  in that category for that month exceeds its budget limit.
- **FR-012**: System MUST recompute summaries and budget-alert status immediately after any
  expense is created, edited, or deleted, so results are always consistent with current data.
- **FR-013**: System MUST persist all expense and budget data so it survives application restarts.

### Key Entities

- **Expense**: A single recorded spending event. Attributes: amount (positive integer, 1–100,000
  INR), category (from the fixed category list), date (not in the future), optional description.
- **Category**: One of the fixed, predefined set of spending categories — Food, Transport,
  Utilities, Entertainment, Other (confirmed, see Clarifications) — used to classify expenses
  and scope budgets.
- **Budget Limit**: A per-category, per-month spending ceiling set by the user, used to determine
  whether an overspending alert should be raised.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can add a new expense and see it reflected in their expense list in under 2
  seconds.
- **SC-002**: 100% of expenses violating the amount or date rules (decimal, zero, negative,
  future-dated) are rejected with a validation error — none are ever persisted.
- **SC-003**: Monthly summary totals exactly match the sum of that month's non-deleted expenses in
  100% of cases, verified by automated contract tests.
- **SC-004**: An overspending alert is produced for 100% of category/month combinations where
  spending exceeds the set budget limit, and for 0% of combinations where it does not.
- **SC-005**: Filtering by category and/or date range returns exactly the matching expenses, with
  no false positives or omissions, in 100% of tested cases.

## Assumptions

- Single-user application: no multi-user accounts, authentication, or authorization are in scope
  for this assignment.
- The predefined category list (Food, Transport, Utilities, Entertainment, Other — confirmed,
  see Clarifications) does not need to be user-editable for this scope.
- "Future-dated" is evaluated against the server's current date; timezone handling beyond using a
  single consistent server timezone is out of scope.
- Budget limits are set per category per calendar month; cross-month or overall (non-category)
  budgets are out of scope.
- Deleted expenses are hard-deleted (not soft-deleted/archived) for this assignment's scope.
- Budget limits support set/replace only; there is no capability to delete/unset a previously
  configured budget limit for a category/month (confirmed, see Clarifications).
