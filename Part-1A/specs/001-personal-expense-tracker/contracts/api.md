# API Contract: Personal Expense Tracker

Base path: `/api`. All request/response bodies are JSON. All validation errors return
`400 Bad Request` with a body of the form `{ "errors": { "<field>": ["<message>"] } }`.

## Expenses

### `POST /api/expenses`

Create an expense (FR-001, FR-004, FR-005, FR-006).

Request:
```json
{ "amount": 500, "category": "Food", "date": "2026-11-12", "description": "Lunch" }
```

Responses:
- `201 Created` with the created expense (including generated `id`).
- `400 Bad Request` if `amount` is not an integer in [1, 100000], `date` is in the future, or
  `category` is not in the fixed category list.

### `GET /api/expenses?category=&from=&to=`

List expenses, optionally filtered by `category` and/or an inclusive `from`/`to` date range
(FR-007). Query params are all optional and may be combined.

Responses:
- `200 OK` with an array of expenses matching the filters.
- `400 Bad Request` if `from` is after `to` (FR-008).

### `PUT /api/expenses/{id}`

Edit an existing expense (FR-002). Same body shape and validation as `POST`.

Responses:
- `200 OK` with the updated expense.
- `400 Bad Request` for the same validation failures as create.
- `404 Not Found` if `id` does not exist.

### `DELETE /api/expenses/{id}`

Delete an existing expense (FR-003).

Responses:
- `204 No Content` on success.
- `404 Not Found` if `id` does not exist.

## Categories

### `GET /api/categories`

Returns the fixed category list, as the single source of truth consumed by the frontend
(research.md decision).

Response:
```json
["Food", "Transport", "Utilities", "Entertainment", "Other"]
```

## Summary

### `GET /api/summary?year=&month=`

Monthly spending summary (FR-009, FR-012).

Response:
```json
{
  "year": 2026,
  "month": 11,
  "total": 700,
  "byCategory": [
    { "category": "Food", "total": 700, "budgetLimit": 1000, "overBudget": false }
  ]
}
```
`budgetLimit` is `null` and `overBudget` is `false` when no limit is set for that category/month
(spec Edge Cases).

## Budgets

### `POST /api/budgets`

Set (or replace) a budget limit for a category/month (FR-010).

Request:
```json
{ "category": "Food", "year": 2026, "month": 11, "limitAmount": 1000 }
```

Responses:
- `200 OK` with the stored budget limit.
- `400 Bad Request` if `limitAmount` fails the same amount rules as expenses, or `category` is
  invalid.

### `GET /api/budgets?year=&month=`

List budget limits set for a given month, for display alongside the summary.
