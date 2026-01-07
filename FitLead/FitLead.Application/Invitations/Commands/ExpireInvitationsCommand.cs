using FitLead.Application.Common;
using MediatR;


namespace FitLead.Application.Invitations.Commands
{
    public sealed record ExpireInvitationsCommand(
        DateTime Now
    ) : IRequest<Result>;
}
