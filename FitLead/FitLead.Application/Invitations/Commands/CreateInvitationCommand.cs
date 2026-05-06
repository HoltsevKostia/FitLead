using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Invitations.Commands
{
    public sealed record CreateInvitationCommand(
        int ExpiresInDays
    ) : IRequest<Result<CreateInvitationResult>>;
}
