using FitLead.Domain.Trainings.Exercises;

namespace FitLead.Api.Exercises.Contracts
{
    public sealed record UpdateExerciseRequest(
        string Name,
        string Description,
        string? MediaUrl,
        MuscleGroup? MuscleGroup,
        Equipment? Equipment
    );
}
