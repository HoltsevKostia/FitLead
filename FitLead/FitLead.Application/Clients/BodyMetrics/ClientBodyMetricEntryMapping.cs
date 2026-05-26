using FitLead.Domain.Clients.BodyMetrics;

namespace FitLead.Application.Clients.BodyMetrics
{
    internal static class ClientBodyMetricEntryMapping
    {
        public static ClientBodyMetricEntryDto ToDto(ClientBodyMetricEntry entry)
        {
            return new ClientBodyMetricEntryDto(
                entry.Id,
                entry.ClientId,
                entry.RecordedAt,
                entry.WeightKg,
                entry.BodyFatPercent,
                entry.ChestCm,
                entry.WaistCm,
                entry.HipsCm,
                entry.ArmCm,
                entry.ThighCm,
                entry.Note,
                entry.CreatedAtUtc,
                entry.UpdatedAtUtc);
        }
    }
}
