# ADR-0016: Internal Identity Service with interim password-based token flow

## Status
Accepted

## Date
2026-03

## Context
FitLead currently needs secure authentication with limited operational complexity and fast delivery cadence.

At this stage:
- the team owns API and identity code in the same solution
- a dedicated external OIDC provider introduces additional infrastructure and operational overhead
- immediate product risk is in auth correctness, token lifecycle safety, and regression prevention

## Decision
Use an internal identity service (ASP.NET Identity + API-owned token issuance) as an interim approach.

The current flow includes:
- password-based login endpoint in API
- access token issuance and refresh token rotation
- refresh token family revocation on reuse/misuse
- integration-first test coverage for critical auth scenarios

Security hardening for this phase:
- support asymmetric JWT signing (RSA) in addition to symmetric signing
- keep strict issuer/audience/lifetime/signing-key validation
- maintain reusable migration path to standardized OIDC/OAuth flows in a future phase

## Rationale for Interim Deviation from OIDC/OAuth
- Educational and transparency value: implementing the flow directly gives explicit understanding of token issuance, validation, rotation, and revocation behavior.
- MVP resource constraints: avoids immediate operational overhead of running and maintaining an external IdP stack in the current phase.
- Migration readiness: API already uses standard JWT Bearer validation boundaries, so migration to external OIDC/OAuth is a moderate, manageable change rather than a full auth rewrite.

## Consequences
Pros:
- lower operational overhead for the current team stage
- full control over token lifecycle behavior and security checks
- fast implementation/iteration with high testability in current architecture

Cons:
- password flow in API does not match Microsoft recommended production approach for long-term architecture
- team owns security-critical identity responsibilities typically delegated to dedicated IdP
- future migration to OIDC/OAuth provider is still required for broader federation/SSO/compliance scenarios

## Migration Intent
Target long-term direction remains standardized OIDC/OAuth (Authorization Code + PKCE with dedicated IdP).

Migration effort is expected to be moderate:
- not a one-line configuration switch
- but also not a full application redesign, because resource server boundaries are already in place

Current decision is explicitly temporary and should be revisited when:
- external client ecosystem grows
- federation/SSO requirements appear
- compliance/security maturity requirements increase

## Related
- [ADR-0014](adr-0014-identity-first-auth-foundation.md)
- [ADR-0015](adr-0015-integration-first-testing-strategy.md)

## Key code references
- [`AuthController.cs`](/FitLead/FitLead.Api/Controllers/AuthController.cs)
- [`JwtTokenService.cs`](/FitLead/FitLead.Api/Identity/JwtTokenService.cs)
- [`JwtSigningKeyResolver.cs`](/FitLead/FitLead.Api/Identity/JwtSigningKeyResolver.cs)
- [`RefreshTokenService.cs`](/FitLead/FitLead.Infrastructure/Identity/RefreshTokenService.cs)
- [`Program.cs`](/FitLead/FitLead.Api/Program.cs)
