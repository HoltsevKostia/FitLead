# ADR-0014: Identity-first auth foundation with domain-identity link

## Status
Accepted

## Date
2026-02

## Context
FitLead requires production-ready authentication architecture without exposing Identity concerns to Domain.

The system already has:
- domain `User` aggregate for business behavior and role state (`Trainer`/`Client`)
- application-level `IUserContext` abstraction used by use-cases

The project needs a clean base for ASP.NET Core Identity while preserving DDD boundaries and avoiding a second user model in Domain.

## Decision
Adopt ASP.NET Core Identity as the authentication foundation in Infrastructure/API only.

Boundaries and ownership:
- Domain `User` remains source of truth for business profile and business role state.
- Identity tables remain source of truth for credentials and login identifiers (email/username/password hash/lockout/security stamp).
- Domain and Application must not reference Identity IDs or `IdentityUser`.

Mapping strategy:
- introduce Infrastructure persistence link `UserIdentityLink` with strict 1:1 mapping:
  - `DomainUserId` (PK, FK -> `users.id`)
  - `IdentityUserId` (UNIQUE, FK -> `AspNetUsers.Id`)
  - `CreatedAtUtc`

Implementation shape:
- `FitLeadDbContext` hosts both domain entities and Identity entities via
  `IdentityDbContext<AppIdentityUser, IdentityRole, string>`.
- Identity services are registered in API composition root:
  `AddIdentityCore<AppIdentityUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<FitLeadDbContext>()`.

Out of scope for this ADR phase:
- auth endpoints (`/auth/login`, `/auth/refresh`, `/auth/logout`)
- refresh token lifecycle
- BFF integration

## Consequences
Pros:
- clean separation between business model and authentication model
- avoids leaking Identity primitives into Domain/Application
- prepares a stable base for JWT/refresh implementation in next phases
- supports future role/claims expansion without DbContext generic-parameter migration

Cons:
- requires explicit mapping management between domain and identity users
- introduces additional persistence object (`UserIdentityLink`)
- email semantics now exist in two contexts (business contact vs login identifier) and require clear application-level rules

## Related
- [ADR-0001](adr-0001-clean-architecture-layering.md)
- [ADR-0003](adr-0003-single-user-aggregate-role-as-state.md)
- [ADR-0006](adr-0006-iusercontext-dev-header-fallback.md)

## Key code references
- [`FitLeadDbContext.cs`](/FitLead/FitLead.Infrastructure/Persistence/FitLeadDbContext.cs)
- [`AppIdentityUser.cs`](/FitLead/FitLead.Infrastructure/Identity/AppIdentityUser.cs)
- [`UserIdentityLink.cs`](/FitLead/FitLead.Infrastructure/Persistence/Models/UserIdentityLink.cs)
- [`UserIdentityLinkConfiguration.cs`](/FitLead/FitLead.Infrastructure/Persistence/Configurations/UserIdentityLinkConfiguration.cs)
- [`Program.cs`](/FitLead/FitLead.Api/Program.cs)
