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
- CSRF protection for unsafe cookie-auth endpoints
- authorization for protected resources
- authenticated user context and claims propagation

This feature is security-critical and acts as the system entry point.

## Scope
In scope:
- registration flow
- login flow
- refresh token flow
- logout flow
- CSRF token issuance flow
- CSRF enforcement for auth POST endpoints
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
- AUTH-COND-11: auth cookies are issued and rotated correctly
- AUTH-COND-12: logout clears auth cookies and invalidates session
- AUTH-COND-13: anonymous CSRF token issuance works for SPA/browser-style clients
- AUTH-COND-14: login rejects missing CSRF token but preserves existing auth contract when CSRF is valid
- AUTH-COND-15: register rejects missing CSRF token but preserves existing validation/business behavior when CSRF is valid
- AUTH-COND-16: refresh rejects missing CSRF token but preserves existing refresh contract when CSRF is valid
- AUTH-COND-17: logout rejects missing CSRF token but preserves existing logout contract when CSRF is valid

## Test Cases
### Implemented (Phase 1)
- AUTH-TC-01: Register valid trainer user -> `201` + auth cookies issued
  - automated by `RegisterTests.Register_WithValidTrainerPayload_ShouldReturnCreatedAndSetAuthCookies`
- AUTH-TC-02: Register duplicate email -> `409` + `auth.email_exists`
  - automated by `RegisterTests.Register_WithDuplicateEmail_ShouldReturnConflictWithAuthEmailExistsErrorCode`
- AUTH-TC-04: Login valid credentials -> `200` + auth cookies issued
  - automated by `LoginTests.Login_WithValidCredentials_ShouldReturnOkAndSetAuthCookies`
- AUTH-TC-05: Login invalid credentials -> `401`
  - automated by `LoginTests.Login_WithInvalidPassword_ShouldReturnUnauthorized`
  - automated by `LoginTests.Login_WithNonExistentEmail_ShouldReturnUnauthorized`
- AUTH-TC-06: Refresh with valid cookie -> `200` + rotated refresh cookie + renewed access cookie
  - automated by `RefreshTokenTests.Refresh_WithValidCookie_ShouldRotateAndIssueNewAuthCookies`
- AUTH-TC-07: Refresh token reuse detection -> `401`, token family revoked
  - automated by `RefreshTokenTests.Refresh_WithReusedRefreshCookie_ShouldRevokeTokenFamily`
- AUTH-TC-08: Unauthorized access to protected endpoint -> `401`
  - automated by `AuthorizationTests.TrainerOnlyEndpoint_WithoutAccessToken_ShouldReturnUnauthorized`
- AUTH-TC-09: Forbidden access without role permissions -> `403`
  - automated by `AuthorizationTests.TrainerOnlyEndpoint_WithClientRole_ShouldReturnForbidden`
- AUTH-TC-10: Current user claims propagation (`id`, `email`, `role`) -> `200`
  - automated by `CurrentUserTests.CurrentUser_WithValidAuthCookies_ShouldReturnIdEmailAndRole`

### Added Validation Case
- AUTH-TC-11: Register invalid email -> `400` + validation problem details
  - automated by `RegisterTests.Register_WithInvalidEmail_ShouldReturnBadRequestWithValidationProblem`

### Added Cookie Contract Cases
- AUTH-TC-12: Register valid user -> issued cookies allow immediate authenticated access
  - automated by `RegisterTests.Register_WithValidPayload_ShouldAllowAccessToCurrentUserUsingIssuedCookies`
- AUTH-TC-13: Logout clears auth cookies and invalidates session
  - automated by `LogoutTests.Logout_WithAuthenticatedSession_ShouldClearAuthCookiesAndInvalidateCurrentUser`

### Added CSRF Coverage
- AUTH-TC-14: Anonymous CSRF token endpoint returns `204`
  - automated by `CsrfTokenTests.GetCsrfToken_Anonymous_ShouldReturnNoContent`
- AUTH-TC-15: CSRF token endpoint sets readable `XSRF-TOKEN` cookie
  - automated by `CsrfTokenTests.GetCsrfToken_ShouldSetReadableXsrfTokenCookie`
- AUTH-TC-16: CSRF token endpoint sets internal antiforgery cookie
  - automated by `CsrfTokenTests.GetCsrfToken_ShouldSetInternalAntiforgeryCookie`
- AUTH-TC-17: CSRF token endpoint does not require authentication
  - automated by `CsrfTokenTests.GetCsrfToken_ShouldNotRequireAuthentication`
- AUTH-TC-18: Login without CSRF -> `400`
  - automated by `LoginCsrfTests.Login_WithoutCsrfToken_ShouldBeRejected`
- AUTH-TC-19: Login with valid CSRF and valid credentials -> existing success contract
  - automated by `LoginCsrfTests.Login_WithValidCsrfToken_AndValidCredentials_ShouldReturnOkAndSetAuthCookies`
- AUTH-TC-20: Login with valid CSRF and invalid credentials -> existing `401` contract
  - automated by `LoginCsrfTests.Login_WithValidCsrfToken_AndInvalidCredentials_ShouldFollowExistingLoginContract`
- AUTH-TC-21: Register without CSRF -> `400`
  - automated by `RegisterCsrfTests.Register_WithoutCsrfToken_ShouldBeRejected`
- AUTH-TC-22: Register with valid CSRF and valid payload -> existing `201` contract
  - automated by `RegisterCsrfTests.Register_WithValidCsrfToken_AndValidPayload_ShouldFollowExistingRegisterContract`
- AUTH-TC-23: Register with valid CSRF and invalid payload -> existing validation/business error
  - automated by `RegisterCsrfTests.Register_WithValidCsrfToken_AndInvalidPayload_ShouldReturnValidationOrBusinessError`
- AUTH-TC-24: Refresh without CSRF -> `400`
  - automated by `RefreshCsrfTests.Refresh_WithoutCsrfToken_ShouldBeRejected`
- AUTH-TC-25: Refresh with valid CSRF and valid refresh cookie -> existing success contract
  - automated by `RefreshCsrfTests.Refresh_WithValidCsrf_AndValidRefreshCookie_ShouldSucceed`
- AUTH-TC-26: Refresh with valid CSRF but missing refresh cookie -> existing `401` contract
  - automated by `RefreshCsrfTests.Refresh_WithValidCsrf_ButMissingRefreshCookie_ShouldFollowExistingAuthContract`
- AUTH-TC-27: Logout without CSRF -> `400`
  - automated by `LogoutCsrfTests.Logout_WithoutCsrfToken_ShouldBeRejected`
- AUTH-TC-28: Logout with valid CSRF -> existing success contract and cookie clearing
  - automated by `LogoutCsrfTests.Logout_WithValidCsrf_ShouldSucceedAndClearCookies`
- AUTH-TC-29: Logout with valid CSRF and no active session -> preserves existing idempotent behavior
  - automated by `LogoutCsrfTests.Logout_WithValidCsrf_ShouldPreserveExistingIdempotentBehavior`

## Priority
Highest priority in this feature:
- authentication/authorization correctness
- token security behavior (rotation/reuse detection)
- CSRF protection for unsafe cookie-auth flows
- access control enforcement

## Expected Artifacts
- stable auth integration test suite
- reproducible execution locally and in CI
- traceability between conditions, cases, and automated tests

## References
- [Policy](/docs/testing/policy.md)
- [Master plan](/docs/testing/master-test-plan.md)
- [Auth controller](/FitLead/FitLead.Api/Auth/AuthController.cs)
- [Refresh token service](/FitLead/FitLead.Infrastructure/Identity/RefreshTokenService.cs)
- Configure JWT bearer authentication in ASP.NET Core:
  https://learn.microsoft.com/uk-ua/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0
