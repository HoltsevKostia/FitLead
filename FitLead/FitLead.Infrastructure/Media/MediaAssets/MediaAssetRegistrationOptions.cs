namespace FitLead.Infrastructure.Media.MediaAssets
{
    public sealed class MediaAssetRegistrationOptions
    {
        public const string SectionName = "MediaAssets";

        public string[] AllowedRegistrationProviders { get; init; } = [];
    }
}
