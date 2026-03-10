using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Deletion;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Application.Common.Time;
using MediatR;
using FitLead.Application.Trainings.TrainingPrograms.Access;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed class DeleteTrainingProgramHandler
    : IRequestHandler<DeleteTrainingProgramCommand, Result>
    {
        private readonly ITrainingProgramLoader _programLoader;
        private readonly ITrainingProgramRepository _programRepository;
        private readonly ITrainingProgramReadRepository _programReadRepository;
        private readonly IDeletionConfirmationTokenService _tokenService;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTrainingProgramHandler(
            ITrainingProgramLoader programLoader,
            ITrainingProgramRepository programRepository,
            ITrainingProgramReadRepository programReadRepository,
            IDeletionConfirmationTokenService tokenService,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _programLoader = programLoader;
            _programRepository = programRepository;
            _programReadRepository = programReadRepository;
            _tokenService = tokenService;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            DeleteTrainingProgramCommand request,
            CancellationToken cancellationToken)
        {
            var programResult = await _programLoader.GetOwnedOrNotFoundAsync(
                request.ProgramId,
                cancellationToken);

            if (programResult.IsFailure)
                return Result.Failure(programResult.Error);

            var usageCount = await _programReadRepository.GetUsageCountAsync(
                request.ProgramId,
                cancellationToken);

            if (usageCount > 0)
            {
                var token = _tokenService.IssueToken(
                    DeletionScope.TrainingProgram,
                    request.ProgramId,
                    usageCount,
                    _clock.UtcNow);

                var metadata = new Dictionary<string, object?>
                {
                    ["usage"] = new { trainingProgramWorkoutCount = usageCount },
                    ["confirmationToken"] = token
                };

                return Result.Failure(Error.Conflict(
                    "training_program.in_use",
                    "Training program contains workouts",
                    metadata));
            }

            var program = programResult.Value;
            _programRepository.Remove(program);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
