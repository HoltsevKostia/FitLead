# ADR-0005: Domain Events with explicit dispatcher after SaveChanges

## Status
Accepted

## Date
2026-01

## Context
We need reactions to domain events without coupling the domain model to infrastructure libraries.

## Decision
- Domain entities raise domain events
- Infrastructure dispatches collected events after `SaveChanges`
- MediatR is used as adapter from domain events to application notifications

Outbox pattern was intentionally deferred to avoid complexity in MVP stage.

## Consequences
Pros:
- domain remains infrastructure-agnostic
- events dispatch after persistence boundary

Cons:
- without Outbox, delivery guarantees are limited

## Related
- [ADR-0002](adr-0002-cqrs-with-mediatr.md)

## Key code references
- [`DomainEventDispatcher.cs`](Fitlead/Fitlead/FitLead.Infrastructure/DomainEventDispatcher.cs)
- [`IDomainEventDispatcher.cs`](Fitlead/Fitlead/FitLead.Application/Common/IDomainEventDispatcher.cs)
