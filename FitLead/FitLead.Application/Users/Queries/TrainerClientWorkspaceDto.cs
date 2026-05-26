namespace FitLead.Application.Users.Queries
{
    public sealed record TrainerClientWorkspaceDto(
        Guid ClientId,
        string Email,
        string FullName);
}
