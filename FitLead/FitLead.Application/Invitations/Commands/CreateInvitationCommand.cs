using FitLead.Application.Common.Results;
using MediatR;


namespace FitLead.Application.Invitations.Commands
{
    public sealed record CreateInvitationCommand(
        Guid ClientId
    ) : IRequest<Result<Guid>>;
}
