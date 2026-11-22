# API Contract: AI Expense Categorization Assistant

Base path: `/api`. All request/response bodies are JSON.

## Extraction

### `POST /api/extract`

Parse a free-text expense statement (FR-001–FR-009).

Request:
```json
{ "message": "I spent ₹500 on groceries and ₹200 on petrol" }
```

Response (`200 OK`):
```json
{
  "expenses": [
    { "amount": 500, "category": "Food", "date": "2026-11-22", "description": "groceries" },
    { "amount": 200, "category": "Transport", "date": "2026-11-22", "description": "petrol" }
  ],
  "clarificationNeeded": false,
  "clarificationQuestion": null
}
```

Response when clarification is required (still `200 OK` — this is a valid, successful
extraction outcome, not an error):
```json
{
  "expenses": [],
  "clarificationNeeded": true,
  "clarificationQuestion": "What amount did you spend? I couldn't find one in your message."
}
```

Error responses:
- `400 Bad Request` — `message` is empty/missing.
- `502 Bad Gateway` — the Gemini API call failed (timeout, quota, network) — distinguished from
  a `200` structurally-invalid response, so harness evidence can separate "model/service
  failure" from "application/evaluation failure" (assignment Common Expectations).

Note: this endpoint never returns a malformed/non-schema-conforming body to the caller — if the
raw Gemini response fails structural validation, the backend retries once with a corrective
instruction, then falls back to a `clarificationNeeded: true` response rather than surfacing
invalid data (see `research.md` / `tasks.md` for the retry-then-fallback implementation task).
