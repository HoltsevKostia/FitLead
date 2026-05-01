using FitLead.Domain.Invitations;

namespace FitLead.Application.Modules.Invitations
{
    public sealed record InvitationModuleDescriptor(
        Guid Id,
        Guid TrainerId,
        Guid ClientId,
        InvitationStatus Status);
}
