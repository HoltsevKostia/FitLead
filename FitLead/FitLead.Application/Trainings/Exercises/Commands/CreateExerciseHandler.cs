using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Application.Modules.Users;
using FitLead.Domain.Users;
using MediatR;
using FitLead.Application.Identity;
using FitLead.Application.Media.MediaAssets.Access;
using FitLead.Domain.Trainings.Exercises;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed class CreateExerciseHandler
    : IRequestHandler<CreateExerciseCommand, Result<Guid>>
    {
        private readonly IUserContext _user;
        private readonly IUsersModule _usersModule;
        private readonly IMediaAssetLoader _mediaAssetLoader;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateExerciseHandler(
            IUserContext user,
            IUsersModule usersModule,
            IMediaAssetLoader mediaAssetLoader,
            IExerciseRepository exerciseRepository,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _usersModule = usersModule;
            _mediaAssetLoader = mediaAssetLoader;
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateExerciseCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _usersModule.GetByIdAsync(
                _user.UserId,
                cancellationToken);

            if (trainer is null)
                return Result<Guid>.Failure(Error.NotFound("trainer.not_found", "Trainer not found"));

            if (trainer.Role != UserRole.Trainer)
                return Result<Guid>.Failure(Error.Forbidden("trainer.required", "User is not a trainer"));

            if (request.MediaAssetId.HasValue)
            {
                var mediaAssetResult = await _mediaAssetLoader.GetOwnedAllowedForExerciseOrNotFoundAsync(
                    _user.UserId,
                    request.MediaAssetId.Value,
                    cancellationToken);
                if (mediaAssetResult.IsFailure)
                {
                    return Result<Guid>.Failure(mediaAssetResult.Error);
                }
            }

            var exerciseResult = Exercise.CreateTrainerExercise(
                _user.UserId,
                request.Name,
                request.Description,
                request.MediaAssetId,
                request.MuscleGroup,
                request.Equipment);
            if (exerciseResult.IsFailure)
                return Result<Guid>.Failure(exerciseResult.Error);

            await _exerciseRepository.AddAsync(exerciseResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(exerciseResult.Value.Id);
        }
    }
}
