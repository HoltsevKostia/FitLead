# ADR-0008: Time abstraction via `IClock` + `TimeProvider`

## Status
Accepted

## Date
2026-02

## Context
Direct `DateTime.UtcNow` usage in handlers/controllers makes tests brittle and creates multiple time sources.

## Decision
- Define `IClock` with `UtcNow` in Application
- Implement in Infrastructure via singleton `TimeProvider`
- Handlers use `_clock.UtcNow`

## Consequences
Pros:
- deterministic testing via clock substitution
- one time source for use-cases

Cons:
- minor abstraction overhead

## Related
- [ADR-0010](adr-0010-dry-run-confirm-delete-with-tokens.md)

## Key code references
- [`IClock.cs`](FitLead/FitLead/FitLead.Application/Common/Time/IClock.cs)
- [`SystemClock.cs`](FitLead/FitLead/FitLead.Infrastructure/Time/SystemClock.cs)
