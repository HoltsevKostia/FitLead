using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetTrainerClientProgramsQuery(Guid ClientId)
        : IRequest<Result<IReadOnlyList<TrainerClientProgramDto>>>;
}
