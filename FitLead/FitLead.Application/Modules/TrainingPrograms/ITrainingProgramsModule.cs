namespace FitLead.Application.Modules.TrainingPrograms
{
    public interface ITrainingProgramsModule
    {
        Task<TrainingProgramModuleDescriptor?> GetByIdAsync(
            Guid programId,
            CancellationToken cancellationToken = default);
    }
}
