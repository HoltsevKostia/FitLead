# ADR-0017: Cookie-based auth API with antiforgery-protected SPA pattern

## Status
Accepted

## Date
2026-05

## Context
FitLead uses a browser-based web client and cookie-based authentication/session flows.

In this model, browsers automatically attach authentication cookies to requests. That makes unsafe cookie-authenticated endpoints vulnerable to CSRF unless requests are additionally validated.

The project already uses:
- ASP.NET Core Identity as auth foundation
- HttpOnly auth cookies for access/refresh flow
- a Next.js frontend that communicates with the API via `fetch`

A consistent CSRF model is required for:
- `register`
- `login`
- `refresh`
- `logout`
- future unsafe cookie-authenticated API endpoints

## Decision
Adopt ASP.NET Core built-in antiforgery for cookie-authenticated unsafe API requests, using the SPA double-submit pattern.

Rollout strategy:
- do not apply global/class-level antiforgery validation initially
- enable `[ValidateAntiForgeryToken]` explicitly per endpoint
- start with auth endpoints and expand to other cookie-authenticated unsafe endpoints later

Token lifecycle:
- CSRF token is bootstrapped through `/auth/csrf-token`
- frontend refreshes the CSRF token after identity changes such as `register`, `login`, and `logout`
- auth cookies remain `HttpOnly`

## Consequences
Pros:
- aligns with ASP.NET Core built-in antiforgery model
- fits SPA/browser client architecture without placing tokens in URLs
- protects unsafe cookie-authenticated endpoints from CSRF
- keeps rollout incremental and testable

Cons:
- adds cross-cutting frontend/backend coordination
- frontend HTTP layer must preserve `credentials: include`
- new unsafe endpoints must explicitly participate in the CSRF contract

## Related
- [ADR-0014](adr-0014-identity-first-auth-foundation.md)
- [ADR-0015](adr-0015-integration-first-testing-strategy.md)

## Key code references
- [`Program.cs`](/FitLead/FitLead.Api/Program.cs)
- [`AuthController.cs`](/FitLead/FitLead.Api/Auth/AuthController.cs)
- [`csrf.ts`](/client/src/lib/api/csrf.ts)
- [`http-client.ts`](/client/src/lib/api/http-client.ts)
- [`auth-api.ts`](/client/src/lib/api/clients/auth-api.ts)
- [`csrf-protection.md`](/docs/security/csrf-protection.md)
