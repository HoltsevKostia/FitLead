using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetTrainerClientWorkspaceHandler
        : IRequestHandler<GetTrainerClientWorkspaceQuery, Result<TrainerClientWorkspaceDto>>
    {
        private readonly ITrainerClientAccessLoader _accessLoader;

        public GetTrainerClientWorkspaceHandler(ITrainerClientAccessLoader accessLoader)
        {
            _accessLoader = accessLoader;
        }

        public async Task<Result<TrainerClientWorkspaceDto>> Handle(
            GetTrainerClientWorkspaceQuery request,
            CancellationToken cancellationToken)
        {
            var accessResult = await _accessLoader.GetAccessibleClientAsync(
                request.ClientId,
                cancellationToken);

            if (accessResult.IsFailure)
            {
                return Result<TrainerClientWorkspaceDto>.Failure(accessResult.Error);
            }

            return Result<TrainerClientWorkspaceDto>.Success(
                new TrainerClientWorkspaceDto(
                    accessResult.Value.ClientId,
                    accessResult.Value.ClientEmail,
                    accessResult.Value.ClientFullName));
        }
    }
}
