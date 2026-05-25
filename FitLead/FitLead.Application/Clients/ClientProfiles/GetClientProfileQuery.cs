using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Clients.ClientProfiles
{
    public sealed record GetClientProfileQuery : IRequest<Result<ClientProfileDto>>;
}
