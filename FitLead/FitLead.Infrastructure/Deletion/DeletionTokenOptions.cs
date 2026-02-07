namespace FitLead.Infrastructure.Deletion
{
    public sealed class DeletionTokenOptions
    {
        public const string SectionName = "DeletionTokens";

        public int LifetimeMinutes { get; set; } = 15;
    }
}
