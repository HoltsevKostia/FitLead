namespace FitLead.Application.Clients.ClientProfiles
{
    public sealed record ClientProfileDto(
        Guid ClientId,
        string? Goal,
        string? ExperienceLevel,
        int? HeightCm,
        string? Limitations,
        string? TrainingPreferences,
        string? AdditionalInfo,
        DateTime? CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
