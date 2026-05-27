using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetTrainerClientProgressQuery(Guid ClientId)
        : IRequest<Result<TrainerClientProgressDto>>;
}
