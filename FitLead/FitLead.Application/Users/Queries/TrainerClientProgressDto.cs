using FitLead.Application.Clients.BodyMetrics;
using FitLead.Application.Clients.ProgressPhotos;

namespace FitLead.Application.Users.Queries
{
    public sealed record TrainerClientProgressDto(
        IReadOnlyList<ClientBodyMetricEntryDto> BodyMetrics,
        IReadOnlyList<ClientProgressPhotoDto> ProgressPhotos);
}
