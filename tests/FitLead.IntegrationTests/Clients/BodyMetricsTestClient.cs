namespace FitLead.IntegrationTests.Clients;

public sealed class BodyMetricsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "body-metrics")
{
    public Task<HttpResponseMessage> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/client/body-metrics", cancellationToken);
    }

    public Task<HttpResponseMessage> CreateAsync(
        DateOnly? recordedAt = null,
        decimal? weightKg = null,
        decimal? bodyFatPercent = null,
        decimal? chestCm = null,
        decimal? waistCm = null,
        decimal? hipsCm = null,
        decimal? armCm = null,
        decimal? thighCm = null,
        string? note = null,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            "/api/client/body-metrics",
            CreateRequest(
                recordedAt,
                weightKg,
                bodyFatPercent,
                chestCm,
                waistCm,
                hipsCm,
                armCm,
                thighCm,
                note),
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> UpdateAsync(
        Guid entryId,
        DateOnly? recordedAt = null,
        decimal? weightKg = null,
        decimal? bodyFatPercent = null,
        decimal? chestCm = null,
        decimal? waistCm = null,
        decimal? hipsCm = null,
        decimal? armCm = null,
        decimal? thighCm = null,
        string? note = null,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Put,
            $"/api/client/body-metrics/{entryId:D}",
            CreateRequest(
                recordedAt,
                weightKg,
                bodyFatPercent,
                chestCm,
                waistCm,
                hipsCm,
                armCm,
                thighCm,
                note),
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> DeleteAsync(
        Guid entryId,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeAsync(
            HttpMethod.Delete,
            $"/api/client/body-metrics/{entryId:D}",
            cancellationToken,
            includeCsrfHeader);
    }

    private static object CreateRequest(
        DateOnly? recordedAt,
        decimal? weightKg,
        decimal? bodyFatPercent,
        decimal? chestCm,
        decimal? waistCm,
        decimal? hipsCm,
        decimal? armCm,
        decimal? thighCm,
        string? note)
    {
        return new
        {
            recordedAt = recordedAt ?? DateOnly.FromDateTime(DateTime.UtcNow),
            weightKg,
            bodyFatPercent,
            chestCm,
            waistCm,
            hipsCm,
            armCm,
            thighCm,
            note
        };
    }
}
