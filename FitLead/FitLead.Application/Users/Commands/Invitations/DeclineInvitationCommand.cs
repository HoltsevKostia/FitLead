using FitLead.Application.Common;
using MediatR;


namespace FitLead.Application.Users.Commands.Invitations
{
    public sealed record DeclineInvitationCommand(
        Guid ClientId,
        Guid InvitationId,
        DateTime Now
    ) : IRequest<Result>;
}
