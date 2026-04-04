# Test Policy

## Purpose
This document defines the testing goals, principles, and direction for FitLead.  
It is the strategic baseline for planning and executing tests.

## Testing Goals
Testing in FitLead is introduced to:
- reduce the risk of defects in critical scenarios
- detect regressions early
- increase confidence when shipping changes
- support safe refactoring and future evolution

Primary goal is not maximum coverage percentage, but rational quality assurance with acceptable cost.

## Testing Principles
FitLead applies a risk-based, incremental testing approach.

Priority goes to areas where failure:
- blocks key user scenarios
- creates security risk
- breaks data integrity
- impacts many downstream features

Testing expands in phases:
- first: critical and already-implemented modules
- later: domain-oriented coverage after domain refactoring stabilizes

## Current Scope
At the current stage, testing prioritizes backend behavior, especially critical business and infrastructure flows.

Domain coverage is not the first priority before planned domain refactoring milestones are completed.

## Test Levels
Multiple test levels are allowed, but priority and depth depend on the current phase plan.

For critical end-to-end backend flows, integration testing is the preferred level because it validates behavior across components together.

## Prioritization Criteria
Highest priority goes to:
- authentication and authorization
- operations that modify system state
- transactional scenarios
- modules that are foundations for upcoming features
- new feature integration paths

## Policy Evolution
This is a baseline policy for the current FitLead stage.
It will evolve as domain model depth, critical modules, and architectural focus evolve.

## References
- [ADR-0015 (integration-first strategy)](/docs/adr/adr-0015-integration-first-testing-strategy.md)
- ASP.NET Core integration tests: https://learn.microsoft.com/aspnet/core/test/integration-tests
- xUnit shared context and fixtures: https://xunit.net/docs/shared-context
