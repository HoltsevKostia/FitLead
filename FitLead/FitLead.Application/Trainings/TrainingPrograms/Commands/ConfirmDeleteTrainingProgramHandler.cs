using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Deletion;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Application.Common.Time;
using MediatR;
using FitLead.Application.Identity;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed class ConfirmDeleteTrainingProgramHandler
    : IRequestHandler<ConfirmDeleteTrainingProgramCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly ITrainingProgramRepository _programRepository;
        private readonly ITrainingProgramReadRepository _programReadRepository;
        private readonly IDeletionConfirmationTokenService _tokenService;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmDeleteTrainingProgramHandler(
            IUserContext user,
            ITrainingProgramRepository programRepository,
            ITrainingProgramReadRepository programReadRepository,
            IDeletionConfirmationTokenService tokenService,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _programRepository = programRepository;
            _programReadRepository = programReadRepository;
            _tokenService = tokenService;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            ConfirmDeleteTrainingProgramCommand request,
            CancellationToken cancellationToken)
        {
            var program = await _programRepository.GetByIdAsync(
                request.ProgramId,
                cancellationToken);

            if (program is null)
                return Result.Failure(Error.NotFound("training_program.not_found", "Training program not found"));

            if (program.TrainerId != _user.UserId)
                return Result.Failure(Error.Forbidden("training_program.forbidden", "Forbidden"));

            if (!_tokenService.TryValidateToken(
                    request.Token,
                    DeletionScope.TrainingProgram,
                    request.ProgramId,
                    _clock.UtcNow,
                    out var payload))
            {
                return Result.Failure(Error.Validation(
                    "training_program.delete.token_invalid",
                    "Invalid or expired deletion token"));
            }

            var usageCount = await _programReadRepository.GetUsageCountAsync(
                request.ProgramId,
                cancellationToken);

            if (usageCount > 0 && usageCount != payload.UsageCount)
            {
                var newToken = _tokenService.IssueToken(
                    DeletionScope.TrainingProgram,
                    request.ProgramId,
                    usageCount,
                    _clock.UtcNow);

                var metadata = new Dictionary<string, object?>
                {
                    ["usage"] = new { trainingProgramWorkoutCount = usageCount },
                    ["confirmationToken"] = newToken
                };

                return Result.Failure(Error.Conflict(
                    "training_program.in_use",
                    "Training program contains workouts",
                    metadata));
            }

            _programRepository.Remove(program);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
