# ADR-0006: IUserContext + dev header fallback as auth foundation (MVP)

## Status
Accepted

## Date
2026-01

## Context
MVP needs ownership/role flows before full JWT/Identity rollout.

## Decision
- Introduce `IUserContext` in Application
- Implement `HttpUserContext` in Infrastructure
- MVP fallback uses `X-User-Id` header
- `[RequireUser]` is temporary authenticated-gate

## Consequences
Pros:
- ownership rules validated before full auth

Cons:
- header fallback is not secure and must be restricted/removed outside Development

## Related
- [ADR-0003](adr-0003-single-user-aggregate-role-as-state.md)

## Key code references
- [`IUserContext.cs`](/Fitlead/Fitlead/FitLead.Application/Common/Identity/IUserContext.cs)
- [`HttpUserContext.cs`](/Fitlead/Fitlead/FitLead.Infrastructure/Identity/HttpUserContext.cs)
