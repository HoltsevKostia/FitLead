using FitLead.Common.Results;
using FitLead.Domain.Media.MediaAssets;

namespace FitLead.Application.Media.MediaAssets.Access
{
    public interface IMediaAssetLoader
    {
        Task<Result<MediaAsset>> GetOwnedOrNotFoundAsync(
            Guid ownerUserId,
            Guid mediaAssetId,
            CancellationToken cancellationToken);

        Task<Result<IReadOnlyList<MediaAsset>>> GetOwnedAllowedForVideoReportOrNotFoundAsync(
            Guid ownerUserId,
            IReadOnlyCollection<Guid> mediaAssetIds,
            CancellationToken cancellationToken);
    }
}
