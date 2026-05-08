# ADR-0018: Token-based trainer invitations with public preview and single-use acceptance

## Status
Accepted

## Date
2026-05

## Context
The original invitation model assumed trainer-to-known-client invitations with background expiration and anti-spam limits.

FitLead instead requires:
- trainer-generated invite links
- no `ClientId` at creation time
- public preview without authentication
- single-use acceptance by an authenticated `Client`
- no raw invite token storage in the database
- future compatibility with downstream integrations (e.g. messenger) without coupling invitations directly to them

The previous invitation ADR no longer matches the implemented product and technical flow.

## Decision
Adopt a token-based invitation model built around opaque random invite links.


Token strategy:
- generate opaque cryptographically strong random token
- expose raw token only in the generated invite URL
- persist only `TokenHash`
- never store raw invite token in the database

Lifecycle model:
- persisted statuses: `Pending`, `Accepted`, `Revoked`
- `Expired` is derived from `Pending && ExpiresAtUtc <= now`
- invite is single-use

API shape:
- trainer creates invite link
- public preview by token does not change state
- accept is available only to authenticated `Client`
- revoke is available only to owning `Trainer`

Acceptance behavior:
- invitation lookup happens by `TokenHash`
- accepting an invitation creates `TrainerClientRelationship`
- relationship creation is handled transactionally in the use-case flow
- invitation module does not create chat/messenger entities directly

Product boundaries:
- reusable public invites are out of MVP scope
- token is not re-disclosed after creation
- trainer workspace may show link only at creation time, not through later read APIs

## Consequences
Pros:
- aligns invitation flow with MVP user experience
- avoids leaking business identifiers in invite URLs
- supports revoke and single-use semantics cleanly
- keeps public preview safe and read-only
- leaves room for future reusable-invite extensions

Cons:
- raw invite URL is recoverable only at creation time
- derived expiry requires consistent read-path logic
- older invitation assumptions and code paths must be retired or superseded

## Supersedes
- [ADR-0004](adr-0004-invitations-lifecycle-expiration-anti-spam.md)

## Related
- [ADR-0005](adr-0005-domain-events-explicit-dispatcher.md)
- [ADR-0015](adr-0015-integration-first-testing-strategy.md)

## Key code references
- [`Invitation.cs`](/FitLead/FitLead.Domain/Invitations/Invitation.cs)
- [`InvitationConfiguration.cs`](/FitLead/FitLead.Infrastructure/Persistence/Configurations/InvitationConfiguration.cs)
- [`InvitationLinkService.cs`](/FitLead/FitLead.Infrastructure/Invitations/InvitationLinkService.cs)
- [`InvitationsController.cs`](/FitLead/FitLead.Api/Invitations/InvitationsController.cs)
- [`AcceptInvitationHandler.cs`](/FitLead/FitLead.Application/Invitations/Commands/AcceptInvitationHandler.cs)
