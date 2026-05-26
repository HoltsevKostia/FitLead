using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Clients.ClientProfiles
{
    public sealed record UpdateClientProfileCommand(
        string? Goal,
        string? ExperienceLevel,
        int? HeightCm,
        string? Limitations,
        string? TrainingPreferences,
        string? AdditionalInfo) : IRequest<Result<ClientProfileDto>>;
}
