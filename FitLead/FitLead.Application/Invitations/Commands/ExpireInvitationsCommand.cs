using FitLead.Application.Common.Results;
using MediatR;


namespace FitLead.Application.Invitations.Commands
{
    public sealed record ExpireInvitationsCommand(
        
    ) : IRequest<Result>;
}
