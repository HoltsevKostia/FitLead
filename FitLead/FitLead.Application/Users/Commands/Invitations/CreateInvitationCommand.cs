using FitLead.Application.Common;
using MediatR;


namespace FitLead.Application.Users.Commands.Invitations
{
    public sealed record CreateInvitationCommand(
        Guid TrainerId,
        Guid ClientId,
        DateTime Now
    ) : IRequest<Result<Guid>>;
}
