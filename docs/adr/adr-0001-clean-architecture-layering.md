# ADR-0001: Clean Architecture layering (Domain/Application/Infrastructure/API)

## Status
Accepted

## Date
2025-12

## Context
We need an architecture that supports long-term maintainability, testability, and dependency control.
The project is expected to grow, so business logic must stay isolated from transport and persistence details.

## Decision
Use Clean Architecture with clear layers:
- Domain: entities, invariants, domain events
- Application: use-cases (CQRS handlers), repository contracts, orchestration
- Infrastructure: EF Core/PostgreSQL, repository implementations, integrations
- API: controllers and HTTP concerns

Dependencies are inward only:
- API -> Application -> Domain
- Infrastructure -> Application/Domain through interfaces

## Consequences
Pros:
- explicit boundaries and lower coupling
- easier replacement of DB/transport details

Cons:
- more abstractions/files

## Related
- [ADR-0002](adr-0002-cqrs-with-mediatr.md)
- [ADR-0012](adr-0012-shared-kernel-fitlead-common.md)
