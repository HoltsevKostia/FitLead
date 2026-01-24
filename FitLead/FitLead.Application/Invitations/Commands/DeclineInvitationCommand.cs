using FitLead.Application.Common.Results;
using MediatR;


namespace FitLead.Application.Invitations.Commands
{
    public sealed record DeclineInvitationCommand(
        Guid ClientId,
        Guid InvitationId,
        DateTime Now
    ) : IRequest<Result>;
}
