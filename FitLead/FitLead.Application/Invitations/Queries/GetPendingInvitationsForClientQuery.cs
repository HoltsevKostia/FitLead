using MediatR;


namespace FitLead.Application.Invitations.Queries
{
    public sealed record GetPendingInvitationsForClientQuery(
        
    ) : IRequest<IReadOnlyList<InvitationDto>>;
}
