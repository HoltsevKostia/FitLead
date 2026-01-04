using MediatR;


namespace FitLead.Application.Invitations.Queries
{
    public sealed record GetSentInvitationsByTrainerQuery(
        Guid TrainerId
    ) : IRequest<IReadOnlyList<InvitationDto>>;
}
