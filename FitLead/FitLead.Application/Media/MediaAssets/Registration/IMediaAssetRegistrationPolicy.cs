using FitLead.Domain.Media.MediaAssets;

namespace FitLead.Application.Media.MediaAssets.Registration
{
    public interface IMediaAssetRegistrationPolicy
    {
        bool IsProviderAllowed(MediaStorageProvider storageProvider);
    }
}
