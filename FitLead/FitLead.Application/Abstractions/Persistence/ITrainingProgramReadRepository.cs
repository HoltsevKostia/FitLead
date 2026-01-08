using FitLead.Application.Trainings.Queries.TrainingProgram;
using FitLead.Application.Trainings.Queries.Workout;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainingProgramReadRepository
    {
        Task<IReadOnlyList<TrainingProgramDto>> GetByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<WorkoutDto>> GetWorkoutsByProgramIdAsync(
            Guid programId, 
            CancellationToken cancellationToken);
    }
}
