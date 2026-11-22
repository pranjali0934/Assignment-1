# Quickstart Validation — Manual End-to-End Run

Executed against the running backend (`dotnet run`, SQL Server `localhost` instance, migration
applied) on 2026-11-13, following `specs/001-personal-expense-tracker/quickstart.md`.

## Scenario 1 — Create and validate an expense

- `POST /api/expenses {"amount":500,"category":"Food","date":"2026-06-01","description":"Lunch"}`
  → `201 Created`, body `{"id":1,"amount":500,"category":"Food","date":"2026-06-01",...}`
- `POST /api/expenses {"amount":500.50,...}` → `400 Bad Request`
- `POST /api/expenses {"amount":500,"date":"2099-01-01",...}` → `400 Bad Request`

## Scenario 2 — Filter expenses

- Created a `Transport` expense dated 2026-06-02.
- `GET /api/expenses?category=Food` → returned only the `Food` expense
- `GET /api/expenses?from=2026-06-01&to=2026-06-01` → returned only the expense on that date

## Scenario 3 — Monthly summary and budget alert

- Created a second `Food` expense of ₹200 (2026-06 total = ₹700).
- `POST /api/budgets {"category":"Food","year":2026,"month":6,"limitAmount":500}` → `200 OK`
- `GET /api/summary?year=2026&month=6` →
  `{"total":900,"byCategory":[{"category":"Food","total":700,"budgetLimit":500,"overBudget":true},{"category":"Transport","total":200,"budgetLimit":null,"overBudget":false}]}`
  (Food over budget, Transport not — matches spec Edge Cases: no alert when no limit is set.)

## Scenario 4 — Edit and delete

- `PUT /api/expenses/1 {"amount":750,...}` → `200 OK`, amount updated.
- `GET /api/summary?year=2026&month=6` immediately after → Food total recalculated to ₹950
  (confirms FR-012: summaries recompute live, no stale/cached alert state)
- `DELETE /api/expenses/2` → `204 No Content`; subsequent `GET /api/expenses` no longer includes
  it

## Result

All 4 quickstart scenarios passed exactly as specified. Test data was cleared afterward
(`dotnet ef database drop` + `dotnet ef database update`) to leave a clean database for the next
run.
