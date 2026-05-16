using FitLead.Application.Media.MediaAssets.Registration;
using FitLead.Domain.Media.MediaAssets;
using Microsoft.Extensions.Options;

namespace FitLead.Infrastructure.Media.MediaAssets
{
    public sealed class MediaAssetRegistrationPolicy : IMediaAssetRegistrationPolicy
    {
        private readonly HashSet<MediaStorageProvider> _allowedProviders;

        public MediaAssetRegistrationPolicy(IOptions<MediaAssetRegistrationOptions> options)
        {
            _allowedProviders = options.Value.AllowedRegistrationProviders
                .Select(value => Enum.TryParse<MediaStorageProvider>(value, true, out var provider)
                    ? provider
                    : (MediaStorageProvider?)null)
                .Where(provider => provider.HasValue && Enum.IsDefined(provider.Value))
                .Select(provider => provider!.Value)
                .ToHashSet();
        }

        public bool IsProviderAllowed(MediaStorageProvider storageProvider)
            => _allowedProviders.Contains(storageProvider);
    }
}
