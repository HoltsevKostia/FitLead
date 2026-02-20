# ADR-0011: Global exception handling via `IExceptionHandler` + ProblemDetails

## Status
Accepted

## Date
2026-02

## Context
`Result` covers expected use-case failures, but infrastructure failures still surface as exceptions.
API must always return a consistent `ProblemDetails` contract.

## Decision
In API:
- configure `AddProblemDetails()`
- register `GlobalExceptionHandler` (`IExceptionHandler`)
- enable `UseExceptionHandler()` middleware

Exception mapping includes EF outcomes:
- `DbUpdateConcurrencyException` -> `409` (`db.concurrency_conflict`)
- unique/FK constraint violations -> `409`
- not-null/check violations -> `400`
- other `DbUpdateException` -> `500` (`db.update_failed`)

`ProblemDetails.Extensions` includes `errorCode` and `traceId`.

## Consequences
Pros:
- consistent error response format
- stable error codes independent of exception messages
- meaningful classification of common database failures

Cons:
- database error classification relies on EF Core provider behavior and translation packages (e.g., Npgsql, EntityFramework.Exceptions)
- updating the database provider or related packages may require adjusting exception mapping logic
- mapping logic is specific to the chosen database (PostgreSQL)

## Related
- [ADR-0009](adr-0009-result-error-problemdetails.md)

## Key code references
- [`GlobalExceptionHandler.cs`](FitLead/FitLead/FitLead.Api/Common/Errors/GlobalExceptionHandler.cs)
