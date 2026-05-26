using FitLead.Domain.Clients.ClientProfiles;

namespace FitLead.Application.Clients.ClientProfiles
{
    internal static class ClientProfileMapping
    {
        public static ClientProfileDto ToDto(ClientProfile profile)
        {
            return new ClientProfileDto(
                profile.ClientId,
                profile.Goal,
                profile.ExperienceLevel?.ToString(),
                profile.HeightCm,
                profile.Limitations,
                profile.TrainingPreferences,
                profile.AdditionalInfo,
                profile.CreatedAtUtc,
                profile.UpdatedAtUtc);
        }

        public static ClientProfileDto Empty(Guid clientId)
        {
            return new ClientProfileDto(
                clientId,
                Goal: null,
                ExperienceLevel: null,
                HeightCm: null,
                Limitations: null,
                TrainingPreferences: null,
                AdditionalInfo: null,
                CreatedAtUtc: null,
                UpdatedAtUtc: null);
        }
    }
}
