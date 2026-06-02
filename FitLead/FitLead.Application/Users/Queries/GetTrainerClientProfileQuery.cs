using FitLead.Application.Clients.ClientProfiles;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetTrainerClientProfileQuery(Guid ClientId)
        : IRequest<Result<ClientProfileDto>>;
}
