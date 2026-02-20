# ADR-0009: Typed Result/Error model + mapping to HTTP ProblemDetails

## Status
Accepted

## Date
2026-02

## Supersedes
- [ADR-0007](adr-0007-result-model-initial-version.md)

## Context
Expected failures need stable and typed contracts for API clients.

## Decision
Use typed outcome primitives:
- `Result` / `Result<T>`
- `Error` (`Code`, `Message`, `Type`, optional `Metadata`)

API maps results centrally via `ResultExtensions` to `ProblemDetails` with stable `errorCode`.

## Consequences
Pros:
- consistent HTTP responses
- stable client-facing error contracts

Cons:
- requires strict error code governance

## Related
- [ADR-0011](adr-0011-global-exception-handling-problemdetails.md)
- [ADR-0012](adr-0012-shared-kernel-fitlead-common.md)

## Key code references
- [`Result.cs`](/FitLead/FitLead/FitLead.Common/Results/Result.cs)
- [`Error.cs`](/FitLead/FitLead/FitLead.Common/Errors/Error.cs)
- [`ResultExtensions.cs`](/FitLead/FitLead/FitLead.Api/Common/Results/ResultExtensions.cs)
