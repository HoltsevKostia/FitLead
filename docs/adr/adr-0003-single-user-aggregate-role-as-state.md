# ADR-0003: Single `User` aggregate with role as state (Trainer/Client)

## Status
Accepted

## Date
2025-12

## Context
Trainer and Client belong to one domain concept: user identity.

## Decision
Keep a single domain aggregate: `User`.
Represent Trainer/Client as `UserRole` state, not separate aggregates/tables.

## Consequences
Pros:
- simpler model and invariants

Cons:
- role checks are explicit in many handlers need to be refactored with auth introduction

## Related
- [ADR-0006](adr-0006-iusercontext-dev-header-fallback.md)

## Key code references
- [`User.cs`](/Fitlead/Fitlead/FitLead.Domain/Users/User.cs)
- [`UserRole.cs`](/Fitlead/Fitlead/FitLead.Domain/Users/UserRole.cs)

