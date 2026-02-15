using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Invitations.Commands
{
    public sealed record AcceptInvitationCommand(
        Guid InvitationId
    ) : IRequest<Result>;
}
