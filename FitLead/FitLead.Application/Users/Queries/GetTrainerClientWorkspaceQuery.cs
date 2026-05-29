using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetTrainerClientWorkspaceQuery(Guid ClientId)
        : IRequest<Result<TrainerClientWorkspaceDto>>;
}
