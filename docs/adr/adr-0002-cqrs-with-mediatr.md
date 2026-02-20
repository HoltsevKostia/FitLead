# ADR-0002: CQRS with MediatR (commands/queries)

## Status
Accepted

## Date
2025-12

## Context
The system has many heterogeneous use-cases. We need explicit request boundaries and consistent handler orchestration.

## Decision
Use CQRS with MediatR:
- Commands mutate state and return `Result` / `Result<T>`
- Queries read state and return `Result<T>`
- Controllers stay thin and delegate execution to MediatR

Policy: MediatR is used for all application use-cases (commands and queries).

## Consequences
Pros:
- explicit use-case boundaries
- better test isolation per handler
- consistent pipeline behavior integration

Cons:
- more files/classes
- potential over-structuring for very simple reads

## Related
- [ADR-0001](adr-0001-clean-architecture-layering.md)
- [ADR-0009](adr-0009-result-error-problemdetails.md)

## Key code references
- [`Result.cs`](FitLead/FitLead.Common/Results/Result.cs)
- [`CreateTrainingProgramCommand.cs`](FitLead/FitLead.Application/Trainings/TrainingPrograms/Commands/CreateTrainingProgramCommand.cs)
- [`GetTrainingProgramsByTrainerIdQuery.cs`](FitLead/FitLead.Application/Trainings/TrainingPrograms/Queries/GetTrainingProgramsByTrainerIdQuery.cs)
