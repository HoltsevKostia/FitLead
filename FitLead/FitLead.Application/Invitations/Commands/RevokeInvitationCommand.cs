using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Invitations.Commands
{
    public sealed record RevokeInvitationCommand(
        Guid InvitationId
    ) : IRequest<Result>;
}
