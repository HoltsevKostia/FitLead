using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetTrainerClientProgramsHandler
        : IRequestHandler<GetTrainerClientProgramsQuery, Result<IReadOnlyList<TrainerClientProgramDto>>>
    {
        private readonly ITrainerClientAccessLoader _accessLoader;
        private readonly ITrainerClientProgramsReadRepository _programsReadRepository;

        public GetTrainerClientProgramsHandler(
            ITrainerClientAccessLoader accessLoader,
            ITrainerClientProgramsReadRepository programsReadRepository)
        {
            _accessLoader = accessLoader;
            _programsReadRepository = programsReadRepository;
        }

        public async Task<Result<IReadOnlyList<TrainerClientProgramDto>>> Handle(
            GetTrainerClientProgramsQuery request,
            CancellationToken cancellationToken)
        {
            var accessResult = await _accessLoader.GetAccessibleClientAsync(
                request.ClientId,
                cancellationToken);

            if (accessResult.IsFailure)
            {
                return Result<IReadOnlyList<TrainerClientProgramDto>>.Failure(accessResult.Error);
            }

            var programs = await _programsReadRepository.GetProgramsAsync(
                accessResult.Value.TrainerId,
                accessResult.Value.ClientId,
                cancellationToken);

            return Result<IReadOnlyList<TrainerClientProgramDto>>.Success(programs);
        }
    }
}
