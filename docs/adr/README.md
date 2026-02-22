# Architecture Decision Records (ADR)

This directory contains short records of architectural decisions for FitLead.
Each ADR captures one decision: context, decision, consequences.

## Index

| ADR | Title | Status | Date |
|---:|---|---|---|
| [0001](adr-0001-clean-architecture-layering.md) | Clean Architecture layering (Domain/Application/Infrastructure/API) | Accepted | 2025-12 |
| [0002](adr-0002-cqrs-with-mediatr.md) | CQRS with MediatR (commands/queries) | Accepted | 2025-12 |
| [0003](adr-0003-single-user-aggregate-role-as-state.md) | Single `User` aggregate with role as state (Trainer/Client) | Accepted | 2025-12 |
| [0004](adr-0004-invitations-lifecycle-expiration-anti-spam.md) | Invitations lifecycle + background expiration + anti-spam limits | Accepted | 2025-12 |
| [0005](adr-0005-domain-events-explicit-dispatcher.md) | Domain Events with explicit dispatcher after SaveChanges | Accepted | 2026-01 |
| [0006](adr-0006-iusercontext-dev-header-fallback.md) | IUserContext + dev header fallback as auth foundation (MVP) | Accepted | 2026-01 |
| [0007](adr-0007-result-model-initial-version.md) | Initial Result model in Application (without typed Error metadata) | Superseded | 2026-01 |
| [0008](adr-0008-time-abstraction-iclock-timeprovider.md) | Time abstraction via `IClock` (Application) + `TimeProvider` (Infrastructure) | Accepted | 2026-02 |
| [0009](adr-0009-result-error-problemdetails.md) | Typed Result/Error model + mapping to HTTP ProblemDetails | Accepted | 2026-02 |
| [0010](adr-0010-dry-run-confirm-delete-with-tokens.md) | Dry-run + Confirm deletion flow with confirmation tokens | Accepted | 2026-02 |
| [0011](adr-0011-global-exception-handling-problemdetails.md) | Global exception handling via `IExceptionHandler` + ProblemDetails | Accepted | 2026-02 |
| [0012](adr-0012-shared-kernel-fitlead-common.md) | Shared kernel in `FitLead.Common` for framework-agnostic primitives | Accepted | 2026-02 |
| [0013](adr-0013-domain-result-for-expected-failures.md) | Domain uses `Result` for expected business failures | Accepted | 2026-02 |

## ADR status conventions

- Accepted - decision is implemented and in active use
- Superseded - replaced by another ADR (reference required)
- Deprecated - discouraged and planned for removal
