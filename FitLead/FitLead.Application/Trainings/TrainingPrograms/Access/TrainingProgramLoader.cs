using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Trainings;

namespace FitLead.Application.Trainings.TrainingPrograms.Access
{
    public sealed class TrainingProgramLoader : ITrainingProgramLoader
    {
        private readonly ITrainingProgramRepository _programRepository;
        private readonly IUserContext _userContext;

        public TrainingProgramLoader(
            ITrainingProgramRepository programRepository,
            IUserContext userContext)
        {
            _programRepository = programRepository;
            _userContext = userContext;
        }

        public async Task<Result<TrainingProgram>> GetOwnedOrNotFoundAsync(
            Guid programId,
            CancellationToken cancellationToken)
        {
            var currentUserId = _userContext.UserIdOrNull;
            if (!currentUserId.HasValue)
            {
                return Result<TrainingProgram>.Failure(
                    Error.Unauthorized("auth.user_missing", "Current user is missing"));
            }

            var program = await _programRepository.GetByIdAsync(programId, cancellationToken);
            if (program is null || program.TrainerId != currentUserId.Value)
            {
                return Result<TrainingProgram>.Failure(
                    Error.NotFound("training_program.not_found", "Training program not found"));
            }

            return Result<TrainingProgram>.Success(program);
        }
    }
}
