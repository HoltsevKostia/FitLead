# ADR-0013: Domain uses Result for expected business failures

## Status
Accepted

## Date
2026-02

## Context
Domain logic contains expected business outcomes that are not exceptional from a system perspective:
validation failures, invariant violations, invalid state transitions, and conflict conditions inside aggregate behavior.

Representing these expected outcomes through exceptions couples normal domain flow to exception infrastructure and makes intent less explicit across layers.

The solution already uses typed `Result` and `Error` contracts as a cross-layer language for failures.

## Decision
Adopt `Result` / `Result<T>` as the architectural contract for expected business failures in Domain.

Exceptions remain reserved for unexpected failures only (runtime faults, infrastructure failures, programming defects, and other non-business exceptional conditions).

This establishes a consistent failure model:
- Domain expresses expected failures as typed results.
- Application orchestrates and propagates those results.
- API maps results to transport-specific response shapes.

## Consequences
Pros:
- explicit and predictable domain behavior for expected outcomes
- consistent failure semantics across Domain -> Application -> API
- reduced reliance on exception-based control flow for normal business scenarios
- improved testability of business rules as deterministic outcomes

Cons:
- migration period may include mixed styles until all modules are aligned
- more explicit result-handling code in orchestration layers
- requires discipline in error code design

## Related
- [ADR-0009](adr-0009-result-error-problemdetails.md)
- [ADR-0011](adr-0011-global-exception-handling-problemdetails.md)
- [ADR-0012](adr-0012-shared-kernel-fitlead-common.md)

## Key code references
- [`Result.cs`](/FitLead/FitLead.Common/Results/Result.cs)
- [`Error.cs`](/FitLead/FitLead.Common/Errors/Error.cs)
- [`Workout.cs`](/FitLead/FitLead.Domain/Trainings/Workout.cs)
- [`DomainExceptionToResultBehavior.cs`](/FitLead/FitLead.Application/Common/Results/DomainExceptionToResultBehavior.cs)
