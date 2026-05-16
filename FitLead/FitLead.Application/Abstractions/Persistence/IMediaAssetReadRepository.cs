using FitLead.Application.Media.MediaAssets.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IMediaAssetReadRepository
    {
        Task<IReadOnlyList<MediaAssetDto>> GetActiveOwnedByUserAsync(
            Guid ownerUserId,
            CancellationToken cancellationToken);
    }
}
