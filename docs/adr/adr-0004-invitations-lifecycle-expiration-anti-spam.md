# ADR-0004: Invitations lifecycle + background job

## Status
Accepted

## Date
2025-12

## Context
We need a controlled trainer-to-client invitation process with deduplication, anti-spam, and automatic expiration.

## Decision
- Invitation aggregate statuses: `Pending` / `Accepted` / `Declined` / `Expired`
- Domain rules:
  - no duplicate pending invitations for trainer-client pair
  - Invitations are subject to rate limiting to prevent abuse.
The concrete threshold is configurable and currently set to a conservative value for MVP validation.
  - ownership checks between trainer and client actions
- Background worker expires overdue invitations
- Separate "mine" read endpoints for trainer and client

## Consequences
Pros:
- controlled lifecycle and transitions
- reduced spam/duplicates

Cons:
- background worker needs production hardening (retries/observability)

## Related
- [ADR-0005](adr-0005-domain-events-explicit-dispatcher.md)

## Key code references
- [`Invitation.cs`](/Fitlead/Fitlead/FitLead.Domain/Invitations/Invitation.cs)
- [`InvitationExpirationWorker.cs`](/Fitlead/Fitlead/FitLead.Infrastructure/BackgroundJobs/Invitations/InvitationExpirationWorker.cs)
