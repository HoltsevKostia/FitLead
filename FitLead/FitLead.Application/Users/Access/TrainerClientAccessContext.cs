namespace FitLead.Application.Users.Access
{
    public sealed record TrainerClientAccessContext(
        Guid TrainerId,
        Guid ClientId,
        string ClientEmail,
        string ClientFullName);
}
