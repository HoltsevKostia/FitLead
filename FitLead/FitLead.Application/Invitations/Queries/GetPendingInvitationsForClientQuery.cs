using MediatR;


namespace FitLead.Application.Invitations.Queries
{
    public sealed record GetPendingInvitationsForClientQuery(
        Guid ClientId,
        DateTime Now
    ) : IRequest<IReadOnlyList<InvitationDto>>;
}
