using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Invitations.Queries
{
    public sealed record GetSentInvitationsByTrainerQuery(
        
    ) : IRequest<Result<IReadOnlyList<InvitationDto>>>;
}
