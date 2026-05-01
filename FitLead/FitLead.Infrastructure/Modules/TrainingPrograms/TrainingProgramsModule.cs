using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Modules.TrainingPrograms;

namespace FitLead.Infrastructure.Modules.TrainingPrograms
{
    public sealed class TrainingProgramsModule : ITrainingProgramsModule
    {
        private readonly ITrainingProgramRepository _trainingProgramRepository;

        public TrainingProgramsModule(ITrainingProgramRepository trainingProgramRepository)
        {
            _trainingProgramRepository = trainingProgramRepository;
        }

        public async Task<TrainingProgramModuleDescriptor?> GetByIdAsync(
            Guid programId,
            CancellationToken cancellationToken = default)
        {
            var program = await _trainingProgramRepository.GetByIdAsync(programId, cancellationToken);
            if (program is null)
                return null;

            return new TrainingProgramModuleDescriptor(program.Id, program.TrainerId);
        }
    }
}
