using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Clients.BodyMetrics
{
    public sealed record CreateClientBodyMetricEntryCommand(
        DateOnly RecordedAt,
        decimal? WeightKg,
        decimal? BodyFatPercent,
        decimal? ChestCm,
        decimal? WaistCm,
        decimal? HipsCm,
        decimal? ArmCm,
        decimal? ThighCm,
        string? Note) : IRequest<Result<ClientBodyMetricEntryDto>>;
}
