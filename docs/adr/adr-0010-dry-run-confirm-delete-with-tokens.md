# ADR-0010: Dry-run + Confirm deletion flow with confirmation tokens

## Status
Accepted

## Date
2026-02

## Context
Cascade delete is not allowed for shared aggregates (Exercise/Workout) because they can be referenced by other aggregates.
With FK Restrict, deletion must be controlled and UX-friendly.

## Decision
Two-step delete flow:

1. Dry-run (`DELETE`)
- if resource has dependencies -> `409 Conflict`
- `ProblemDetails` includes:
  - `errorCode`
  - `usage` (counts)
  - `confirmationToken` (time-limited token)
- no state mutation in dry-run

2. Confirm (`POST .../deletion-confirmations`)
- validate token (scope + targetId + TTL)
- re-check `usageCount`
- if `usageCount > 0` and differs from snapshot -> `409` + new token (re-confirm)
- detach link entities via bulk delete (`ExecuteDeleteAsync`)
- delete root aggregate and call `SaveChanges` in one handler

Tokens are implemented using ASP.NET Core Data Protection (time-limited protector).

## Consequences
Pros:
- controlled UX and predictable behavior
- no soft delete required at this stage

Cons:
- production/scale-out requires persistent Data Protection key ring
- workflow is more complex than plain CRUD

## Related
- [ADR-0008](adr-0008-time-abstraction-iclock-timeprovider.md)

## Key code references
- [`IDeletionConfirmationTokenService.cs`](/FitLead/FitLead/FitLead.Application/Common/Deletion/IDeletionConfirmationTokenService.cs)
- [`DeleteExerciseHandler.cs`](/FitLead/FitLead/FitLead.Application/Trainings/Exercises/Commands/DeleteExerciseHandler.cs)
- [`ConfirmDeleteExerciseHandler.cs.cs`](/FitLead/FitLead/FitLead.Application/Trainings/Exercises/Commands/ConfirmDeleteExerciseHandler.cs)