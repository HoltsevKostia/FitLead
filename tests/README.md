# FitLead Tests Guide

This folder contains automated tests for FitLead.

Current focus:
- integration-first testing for critical backend scenarios
- feature-scoped tests inside `FitLead.IntegrationTests/Features`

Project-level testing docs:
- policy: [`docs/testing/policy.md`](/FitLead/docs/testing/policy.md)
- master plan: [`docs/testing/master-test-plan.md`](/FitLead/docs/testing/master-test-plan.md)
- feature specs (auth example): [`tests/FitLead.IntegrationTests/Features/Auth/tests.md`](/FitLead/tests/FitLead.IntegrationTests/Features/Auth/tests.md)

## Prerequisites

Before running integration tests, make sure:
- `.NET SDK 8` is installed
- Docker runtime is available (`Docker Desktop` or compatible Docker engine)
- Docker daemon is running

Quick check:
```powershell
docker ps
dotnet --version
```

## How To Run

From repository root:
```powershell
dotnet test tests\FitLead.IntegrationTests\FitLead.IntegrationTests.csproj
```

Run tests for a single feature (example: `Auth`):
```powershell
dotnet test tests\FitLead.IntegrationTests\FitLead.IntegrationTests.csproj --filter "FullyQualifiedName~Features.Auth"
```

Run a single class inside a feature:
```powershell
dotnet test tests\FitLead.IntegrationTests\FitLead.IntegrationTests.csproj --filter "FullyQualifiedName~Features.Auth.RegisterTests"
```

## Test Naming Convention

Use:
`Feature_Scenario_ExpectedResult`

Examples:
- `Register_WithInvalidEmail_ShouldReturnBadRequestWithValidationProblem`
- `TrainerOnlyEndpoint_WithClientRole_ShouldReturnForbidden`

Rules:
- keep method names explicit and behavior-focused
- include expected HTTP outcome in the name for API tests

## Database Isolation And Cleanup

Integration tests use:
- one PostgreSQL container per test collection (Testcontainers)
- one shared `WebApplicationFactory` per collection
- database reset before each test via Respawn

This means:
- tests are isolated from each other
- no manual DB cleanup is required after tests
- local test data is ephemeral and safe to discard

Note:
- `AspNetRoles` is intentionally preserved between resets because auth tests require seeded roles.

## Project Structure

`FitLead.IntegrationTests` is organized as:
- `Infrastructure/` container, factory, fixtures, DB checkpoint/reset
- `Clients/` HTTP clients/helpers for API interactions
- `Helpers/` shared constants and response parsing helpers
- `Features/` test suites by feature (`Auth`, and later others)

## Adding New Integration Tests

Checklist for new tests:
1. Put tests under `Features/<FeatureName>/`.
2. Reuse existing fixtures and clients; avoid custom startup logic per test class.
3. Use unique test data (`UniqueEmail(...)`, random IDs, etc.).
4. Assert both status code and response contract (`errorCode`, payload fields).
5. Cover positive + negative scenarios for each endpoint/flow.
6. Tests should be stable and repeatable: do not use delays (sleep/delay) and do not rely on execution time or the order of running tests.

## Troubleshooting

`Docker is either not running or misconfigured`
- start Docker Desktop / Docker engine
- verify with `docker ps`

`Jwt signing configuration is missing. Provide RSA key pair`
- ensure both `Jwt:RsaPrivateKeyPem` and `Jwt:RsaPublicKeyPem` are configured (for local API typically via `dotnet user-secrets`)

Intermittent auth failures
- confirm test uses isolated data and does not rely on execution order
- re-run to verify reproducibility; flaky behavior is treated as a bug

## Useful References

- ASP.NET Core integration testing:
  https://learn.microsoft.com/aspnet/core/test/integration-tests
- xUnit shared context and fixtures:
  https://xunit.net/docs/shared-context
- xUnit async lifetime:
  https://xunit.net/docs/shared-context#async-lifetime
- Testcontainers for .NET:
  https://dotnet.testcontainers.org/
- Respawn:
  https://github.com/jbogard/Respawn
