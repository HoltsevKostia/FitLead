namespace FitLead.IntegrationTests.Clients;

public sealed class ProgressPhotosTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "progress-photos")
{
    public Task<HttpResponseMessage> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/client/progress-photos", cancellationToken);
    }

    public Task<HttpResponseMessage> CreateAsync(
        Guid mediaAssetId,
        DateOnly? takenAt = null,
        string? label = "Front",
        string? note = null,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            "/api/client/progress-photos",
            new
            {
                mediaAssetId,
                takenAt = takenAt ?? DateOnly.FromDateTime(DateTime.UtcNow),
                label,
                note
            },
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> DeleteAsync(
        Guid photoId,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeAsync(
            HttpMethod.Delete,
            $"/api/client/progress-photos/{photoId:D}",
            cancellationToken,
            includeCsrfHeader);
    }
}
