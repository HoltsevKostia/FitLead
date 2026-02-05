using FitLead.Application.Common.Results;
using MediatR;

namespace FitLead.Application.Invitations.Queries
{
    public sealed record GetPendingInvitationsForClientQuery(
        
    ) : IRequest<Result<IReadOnlyList<InvitationDto>>>;
}
