# Auth Feature Test Specification

## Purpose
This document defines test scope and test design for the Auth feature in FitLead.

It is used to:
- define test conditions for auth behavior
- define test cases and priorities
- align implementation of automated integration tests

## Feature Summary
Auth feature responsibilities:
- user registration
- user login
- access and refresh token issuance
- access token renewal through refresh flow
- refresh token misuse/reuse protection
- authorization for protected resources
- authenticated user context and claims propagation

This feature is security-critical and acts as the system entry point.

## Scope
In scope:
- registration flow
- login flow
- refresh token flow
- invalid token / token misuse behavior
- authorization behavior (`401` / `403`)
- authenticated user context propagation
- registration consistency in transactional failure scenarios

Out of scope (this phase):
- frontend auth forms behavior
- browser-level E2E
- performance/security penetration testing
- non-auth modules

## Test Level
Primary level: integration testing.

Rationale:
- auth behavior crosses API + identity + persistence + security configuration boundaries
- correctness depends on component interaction, not isolated class behavior only

## Test Conditions
- AUTH-COND-01: successful registration with valid payload
- AUTH-COND-02: duplicate registration is rejected
- AUTH-COND-03: registration remains consistent on failure (no partial state)
- AUTH-COND-04: successful login with valid credentials
- AUTH-COND-05: invalid login is rejected
- AUTH-COND-06: successful refresh token rotation
- AUTH-COND-07: refresh token reuse/revoked token is rejected
- AUTH-COND-08: protected endpoint without token is rejected (`401`)
- AUTH-COND-09: authenticated user without required role is rejected (`403`)
- AUTH-COND-10: authenticated user context/claims are correctly propagated

## Test Cases
### Implemented (Phase 1)
- AUTH-TC-01: Register valid trainer user -> `201` + access/refresh tokens
  - automated by `RegisterTests.Register_WithValidTrainerPayload_ShouldReturnCreatedWithTokens`
- AUTH-TC-02: Register duplicate email -> `409` + `auth.email_exists`
  - automated by `RegisterTests.Register_WithDuplicateEmail_ShouldReturnConflictWithAuthEmailExistsErrorCode`
- AUTH-TC-04: Login valid credentials -> `200` + access/refresh tokens
  - automated by `LoginTests.Login_WithValidCredentials_ShouldReturnOkWithTokens`
- AUTH-TC-05: Login invalid credentials -> `401`
  - automated by `LoginTests.Login_WithInvalidPassword_ShouldReturnUnauthorized`
  - automated by `LoginTests.Login_WithNonExistentEmail_ShouldReturnUnauthorized`
- AUTH-TC-06: Refresh with valid token -> `200` + rotated refresh token + valid access token
  - automated by `RefreshTokenTests.Refresh_WithValidToken_ShouldRotateAndReturnNewTokens`
- AUTH-TC-07: Refresh token reuse detection -> `401`, token family revoked
  - automated by `RefreshTokenTests.Refresh_WithReusedToken_ShouldRevokeTokenFamily`
- AUTH-TC-08: Unauthorized access to protected endpoint -> `401`
  - automated by `AuthorizationTests.TrainerOnlyEndpoint_WithoutAccessToken_ShouldReturnUnauthorized`
- AUTH-TC-09: Forbidden access without role permissions -> `403`
  - automated by `AuthorizationTests.TrainerOnlyEndpoint_WithClientRole_ShouldReturnForbidden`
- AUTH-TC-10: Current user claims propagation (`sub`, `email`, `jti`) -> `200`
  - automated by `CurrentUserTests.CurrentUser_WithValidAccessToken_ShouldReturnSubEmailAndJti`

### Added Validation Case
- AUTH-TC-11: Register invalid email -> `400` + validation problem details
  - automated by `RegisterTests.Register_WithInvalidEmail_ShouldReturnBadRequestWithValidationProblem`

### Planned Next
- AUTH-TC-03: registration rollback / no partial state under induced failure
  - planned; requires controlled failure point for deterministic assertion

## Priority
Highest priority in this feature:
- authentication/authorization correctness
- token security behavior (rotation/reuse detection)
- access control enforcement

## Expected Artifacts
- stable auth integration test suite
- reproducible execution locally and in CI
- traceability between conditions, cases, and automated tests

## References
- [Policy](/docs/testing/policy.md)
- [Master plan](/docs/testing/master-test-plan.md)
- [Auth controller](/FitLead/FitLead.Api/Controllers/AuthController.cs)
- [Refresh token service](/FitLead/FitLead.Infrastructure/Identity/RefreshTokenService.cs)
- Configure JWT bearer authentication in ASP.NET Core:
  https://learn.microsoft.com/uk-ua/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0
