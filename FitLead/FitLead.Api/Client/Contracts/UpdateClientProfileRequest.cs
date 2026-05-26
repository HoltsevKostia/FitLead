namespace FitLead.Api.Client.Contracts
{
    public sealed record UpdateClientProfileRequest(
        string? Goal,
        string? ExperienceLevel,
        int? HeightCm,
        string? Limitations,
        string? TrainingPreferences,
        string? AdditionalInfo);
}
