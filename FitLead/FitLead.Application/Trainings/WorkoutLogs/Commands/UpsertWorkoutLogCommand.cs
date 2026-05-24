using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.WorkoutLogs.Commands
{
    public sealed record UpsertWorkoutLogCommand(
        Guid AssignmentId,
        Guid TrainingProgramWorkoutId,
        string? Status,
        DateTime? PerformedAtUtc,
        string? ClientNote,
        int? DifficultyRating) : IRequest<Result<WorkoutLogDto>>;
}
