# ADR-0007: Initial Result model in Application (without typed Error metadata)

## Status
Superseded

## Date
2026-01

## Superseded by
- [ADR-0009](adr-0009-result-error-problemdetails.md)

## Context
Initial decision introduced `Result`/`Result<T>` to avoid exception-driven flow for expected outcomes.
At this stage, errors were less structured and did not provide the final typed metadata model.

Reason for superseding: need for structured error metadata and consistent API contracts.

## Decision
Use Result-based handler responses as initial standard for commands and queries.

## Consequences
Pros:
- reduced exception usage for expected flow

Cons:
- limited error typing and weaker API error contracts

## Key code references
- [`Result.cs`](Fitlead/Fitlead/FitLead.Common/Results/Result.cs)
