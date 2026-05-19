namespace FitLead.Infrastructure.Media.Uploadcare
{
    public sealed class UploadcareOptions
    {
        public const string SectionName = "Uploadcare";

        public string PublicKey { get; init; } = string.Empty;
        public string SecretKey { get; init; } = string.Empty;
        public int UploadSignatureLifetimeMinutes { get; init; } = 30;
    }
}
