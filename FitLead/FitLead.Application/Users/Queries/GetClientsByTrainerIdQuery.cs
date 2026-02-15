using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetClientsByTrainerIdQuery(

    ) : IRequest<Result<IReadOnlyList<TrainerClientDto>>>;
}

