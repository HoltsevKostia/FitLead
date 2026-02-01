using MediatR;


namespace FitLead.Application.Invitations.Queries
{
    public sealed record GetSentInvitationsByTrainerQuery(
        
    ) : IRequest<IReadOnlyList<InvitationDto>>;
}
