using FitLead.Domain.Trainings;

namespace FitLead.Application.Trainings.Exercises.Queries
{
    public sealed record ExerciseDto(
        Guid Id,
        string Name,
        string Description,
        string? MediaUrl,
        MuscleGroup? MuscleGroup,
        Equipment? Equipment,
        ExerciseSource Source,
        bool IsEditable
    );
}
