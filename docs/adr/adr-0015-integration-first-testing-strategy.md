# ADR-0015: Integration-first testing strategy with feature-scoped specifications

## Status
Accepted

## Date
2026-03

## Context
FitLead already has a layered architecture (Domain/Application/Infrastructure/API) and includes several critical cross-layer behaviors:
- authentication and authorization flows
- transactional write scenarios
- persistence rules enforced by PostgreSQL/EF Core/Identity

At this stage, the highest product risk is regression in end-to-end backend behavior, not isolated algorithmic logic.

A testing direction is needed that:
- reduces risk in critical scenarios first
- stays aligned with iterative delivery
- avoids premature investment in broad, brittle, or low-value tests

## Decision
Adopt an integration-first, feature-first testing strategy for the current phase.

Decision boundaries:
- Primary quality gate for critical backend scenarios is integration testing.
- Test scope is prioritized by feature risk and business impact.
- Detailed scenario definitions live in feature test specifications.
- Unit tests are allowed as a targeted complement for stable, isolated logic, but are not the primary strategy for critical flows in this phase.
- Integration environment is provisioned with Testcontainers to keep database setup reproducible and close to production without manual local provisioning.

Architectural intent:
- verify real interaction across API, application orchestration, persistence, Identity, and database constraints
- keep testing aligned with module maturity (critical and stable modules first)
- preserve ability to evolve toward broader domain-focused unit coverage after current refactoring phases stabilize

## Consequences
Pros:
- strong confidence in cross-component behavior where most current risk exists
- early regression protection for auth/authorization and transactional scenarios
- lower risk of false confidence from overly mocked tests
- strategy matches iterative delivery and feature prioritization
- Testcontainers reduces environment drift and improves repeatability of integration runs across local and CI.

Cons:
- slower test execution compared to pure unit tests
- higher setup/maintenance cost for integration infrastructure
- failures can be harder to localize than unit-level failures
- requires discipline to keep scope focused and avoid uncontrolled test-suite growth

## Trade-offs considered
Alternative A: unit-test-first as the default strategy.
- Rejected for this phase because it under-validates integration contracts and infrastructure boundaries that currently carry the highest risk. Also, Domain invariants will be upgraded with VO and better validation later in the project.

Alternative B: broad E2E/browser-first strategy.
- Rejected for this phase because it adds high cost and flakiness before backend integration guarantees are stable.

Alternative C: coverage-first target (maximize percentage quickly).
- Rejected because percentage alone does not correlate with risk reduction; priority is critical scenario protection.
