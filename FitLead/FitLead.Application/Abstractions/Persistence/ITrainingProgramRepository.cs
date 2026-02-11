using FitLead.Domain.Trainings;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainingProgramRepository
    {
        Task AddAsync(TrainingProgram program, CancellationToken cancellationToken);
        Task<TrainingProgram?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task DeleteTrainingProgramWorkoutsByProgramIdAsync(Guid programId, CancellationToken cancellationToken);
        void Remove(TrainingProgram program);
    }
}
