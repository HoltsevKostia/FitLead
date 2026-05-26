namespace FitLead.Api.Client.Contracts
{
    public sealed record BodyMetricEntryRequest(
        DateOnly RecordedAt,
        decimal? WeightKg,
        decimal? BodyFatPercent,
        decimal? ChestCm,
        decimal? WaistCm,
        decimal? HipsCm,
        decimal? ArmCm,
        decimal? ThighCm,
        string? Note);
}
