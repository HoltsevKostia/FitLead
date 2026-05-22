using FitLead.Application.Notifications.Push;

namespace FitLead.Infrastructure.Notifications.Push
{
    public sealed class PushOptions : IPushVapidConfiguration
    {
        public const string SectionName = "Push";

        public bool Enabled { get; init; } = true;
        public string? VapidPublicKey { get; init; }
        public string? VapidPrivateKey { get; init; }
        public string? Subject { get; init; }

        public string? PublicKey => VapidPublicKey;
    }
}
