# Master Test Plan

## Goal
This document describes how testing is organized in FitLead.
It defines strategy, scope, tooling, environment, process, entry/exit criteria.

## Context
Current architecture and stack:
- Clean Architecture
- Domain-Driven Design (DDD)
- CQRS + MediatR
- ASP.NET Core Web API
- EF Core + PostgreSQL

Delivery is iterative, so testing is also introduced incrementally.

## Scope
Testing currently targets backend behavior, including:
- application business logic
- domain behavior with business impact
- persistence behavior
- authentication and authorization
- component interaction across layers

Detailed scope for each module is documented in feature-level test specifications.

## Out Of Scope (Current Phase)
- full coverage of simple CRUD without business logic
- browser-level E2E testing
- frontend UI testing

These may be introduced in future phases.

## Strategy
FitLead uses a feature-first, integration-first strategy.

This means:
- start from critical, stable, already delivered modules
- validate real cross-layer behavior first
- avoid broad low-value testing on unstable code paths

## Test Environment
Target is a production-like integration environment:
- ASP.NET Core test host (`WebApplicationFactory`)
- PostgreSQL
- containerized dependencies (`Testcontainers`)

## Tooling
- xUnit
- FluentAssertions
- ASP.NET Core `WebApplicationFactory`
- Testcontainers for .NET
- PostgreSQL
- Respawn (database reset between tests)
- coverlet (coverage collection/reporting)
- NSubstitute / Moq (only when needed)

## Implementation Process
Each feature follows:
1. Test Analysis (conditions)
2. Test Design (cases)
3. Test Implementation (automated tests)

## Feature Test Specifications
Each important module should have a feature-level test spec with:
- feature description
- scope
- test conditions
- test cases
- priorities

## Code Coverage
Coverage is used as a feedback signal, not a standalone quality goal.

Coverage approach:
- collect with `coverlet` in CI and local runs when needed
- track trends and blind spots over time
- use coverage reports to find untested critical paths

Coverage target principle:
- not "100% of all lines"
- target strong coverage for critical business and security scenarios
- prioritize scenario-level confidence over synthetic percentage growth

## Risks
- uncontrolled test scope expansion
- brittle tests tied to unstable internals
- overuse of mocks for integration-relevant behavior
- slower delivery due to low-value test volume

## Entry Criteria
Testing for a feature starts when:
- feature scope and priorities are defined in a feature test specification
- API contracts and expected behavior are stable enough for automation
- required test infrastructure is available (test host, database, container runtime)

## Exit Criteria
Testing for a feature is considered complete for the phase when:
- all high-priority planned tests for the phase are automated and passing
- no unresolved critical defects remain in covered scenarios
- integration tests pass in CI with stable execution
- coverage report is generated and reviewed for critical-scenario blind spots

## References
- [Test Policy](/docs/testing/policy.md)
- [Auth feature spec](/FitLead/FitLead.Api/Features/Auth/tests.md)
- [Integration test developer guide](/tests/README.md)
- [ADR-0015](/docs/adr/adr-0015-integration-first-testing-strategy.md)
- coverlet docs: https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/MSBuildIntegration.md
