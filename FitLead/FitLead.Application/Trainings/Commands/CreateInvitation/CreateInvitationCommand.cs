using FitLead.Application.Common;
using MediatR;


namespace FitLead.Application.Trainings.Commands.CreateInvitation
{
    public sealed record CreateInvitationCommand(
        Guid TrainerId,
        Guid ClientId,
        DateTime Now
    ) : IRequest<Result<Guid>>;
}
