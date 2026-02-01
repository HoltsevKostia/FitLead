using FitLead.Application.Common.Results;
using MediatR;


namespace FitLead.Application.Invitations.Commands
{
    public sealed record DeclineInvitationCommand(
        Guid InvitationId
    ) : IRequest<Result>;
}
