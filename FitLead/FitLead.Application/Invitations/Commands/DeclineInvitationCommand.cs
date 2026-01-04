using FitLead.Application.Common;
using MediatR;


namespace FitLead.Application.Invitations.Commands
{
    public sealed record DeclineInvitationCommand(
        Guid ClientId,
        Guid InvitationId,
        DateTime Now
    ) : IRequest<Result>;
}
