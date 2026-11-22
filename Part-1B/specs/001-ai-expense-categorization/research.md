# Phase 0 Research: AI Expense Categorization Assistant

No `[NEEDS CLARIFICATION]` markers remained in the Technical Context — the stack and LLM
provider were fixed by the constitution and user decision. This document records the remaining
implementation decisions and their rationale.

## Decision: Gemini access via direct REST calls (no SDK)

**Decision**: Call the Gemini `generateContent` REST endpoint
(`https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent`) directly
via `HttpClient`, authenticating with the API key as a query parameter or `x-goog-api-key`
header, rather than pulling in a full SDK package.

**Rationale**: The call surface needed is small (one endpoint, with a `responseSchema` /
controlled-generation configuration for structured JSON output) and a direct REST call keeps
the dependency footprint minimal and the request/response shape fully visible and testable.
Google's `response_mime_type: "application/json"` + `response_schema` controlled-generation
feature lets Gemini itself enforce the output JSON shape at generation time, which is Layer 1
of our harness's defense-in-depth (Layer 2's independent validation still re-checks everything,
per Constitution Principle III, in case the model's own schema enforcement is imperfect).

**Alternatives considered**: A community Gemini .NET SDK package — rejected to avoid taking a
dependency on a third-party package with its own versioning/compatibility risk, for a single
small endpoint.

## Decision: Model selection — Gemini Flash tier, with "thinking" disabled

**Decision**: Use a Gemini "Flash" tier model (fast, low-cost) for extraction, configurable via
`GeminiOptions.Model` rather than hardcoded, so it can be swapped if needed. Current default:
`gemini-3.5-flash`, with `generationConfig.thinkingConfig.thinkingBudget: 0` to disable the
model's default extended-reasoning ("thinking") mode.

**Rationale**: This is a structured-extraction task (not open-ended generation), well within a
smaller/faster model's capability, and keeps per-request latency low (spec SC-005: <5s) and
evaluation-run cost minimal for a ≥15-case dataset. Live testing (see harness-log.md §3)
revealed the original plan's `gemini-2.5-flash` had been deprecated for new API keys, and the
API's own suggested replacement (`gemini-3.8-flash`) was unstable (intermittent 503s/hangs) at
test time — `gemini-3.5-flash` responded reliably. Every current-generation Gemini model has
"thinking" enabled by default, which added ~15s of latency for a trivial extraction with no
benefit to this bounded task, so it is explicitly disabled.

**Alternatives considered**: A larger/more capable Gemini tier — rejected as unnecessary cost
and latency for a bounded structured-extraction task. Leaving "thinking" enabled — rejected
after live testing showed ~18s latency for a single-expense extraction, far outside SC-005's
budget, for no measurable accuracy benefit on this task. Note: even with thinking disabled,
live-measured latency remained variable (3–14s) due to external service congestion for this
very new model generation — SC-005 is not reliably met under current conditions (documented as
a known limitation in harness-log.md, not silently accepted).

## Decision: No persistence layer

**Decision**: This feature has no database; `GeminiClient`/`ExtractionService` are stateless
per request.

**Rationale**: Per spec Assumptions, extracted expenses are not automatically persisted into
Part 1A's store in this assignment's scope — Part 1A and Part 1B are evaluated as separate
applications. Removing persistence entirely keeps Part 1B focused on the thing being assessed
(extraction + evaluation), avoiding an unnecessary dependency on SQL Server for this part.

**Alternatives considered**: Reusing Part 1A's `ExpenseTrackerDbContext`/SQL Server — rejected;
would couple two independently-evaluated parts of the assignment together for no requirement
that calls for it.

## Decision: Gemini client testability — interface + fake for contract tests

**Decision**: Define an `IGeminiClient` interface; contract tests inject a fake implementation
returning canned JSON responses (valid, malformed, adversarial, etc.) rather than calling the
real API in every test run.

**Rationale**: Keeps `dotnet test` fast, deterministic, and runnable without a Gemini API key
or network access (important for anyone grading this submission without their own key) — while
the real, non-deterministic evaluation against `evals/dataset.json` is a separate,
explicitly-run harness pass (`.harness/evaluator/`) that does call the live API.

**Alternatives considered**: Only testing via the live API in all test runs — rejected; would
make `dotnet test` flaky, slow, cost money per run, and non-runnable without a key, none of
which fits the "deterministic test suite" expectation for the structural/invariant layers
(Layers 1–2 are deterministic logic and should be tested as such).

## Output

All Technical Context items are resolved; no outstanding `[NEEDS CLARIFICATION]` markers
remain. Proceeding to Phase 1 (data model, contracts, quickstart).
