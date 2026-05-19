using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetMyTrainerQuery(

    ) : IRequest<Result<ClientTrainerDto>>;
}
