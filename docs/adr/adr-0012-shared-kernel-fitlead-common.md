# ADR-0012: Shared kernel in `FitLead.Common` for framework-agnostic primitives

## Status
Accepted

## Date
2026-02

## Context
Shared cross-layer abstractions were anchored in layer-specific projects, increasing coupling and ownership ambiguity.

## Decision
Introduce `FitLead.Common` as shared kernel for framework-agnostic primitives:
- `Result`, `Result<T>`
- `Error`, `ErrorType`
- `Entity<TId>`, `ValueObject`, `IDomainEvent`, `AggregateRoot<TId>`

`FitLead.Common` must not depend on ASP.NET Core, EF Core, MediatR, or infrastructure concerns.

## Consequences
Pros:
- cleaner ownership of shared primitives
- lower cross-layer coupling

Cons:
- requires discipline to avoid becoming a dumping ground

## Related
- [ADR-0001](adr-0001-clean-architecture-layering.md)
- [ADR-0009](adr-0009-result-error-problemdetails.md)

## Key code references
- [`FitLead.Common`](FitLead/FitLead//FitLead.Common)
