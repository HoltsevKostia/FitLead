namespace FitLead.Application.Clients.BodyMetrics
{
    public sealed record ClientBodyMetricEntryDto(
        Guid Id,
        Guid ClientId,
        DateOnly RecordedAt,
        decimal? WeightKg,
        decimal? BodyFatPercent,
        decimal? ChestCm,
        decimal? WaistCm,
        decimal? HipsCm,
        decimal? ArmCm,
        decimal? ThighCm,
        string? Note,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
