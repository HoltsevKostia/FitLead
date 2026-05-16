namespace FitLead.IntegrationTests.Clients;

public sealed class MediaAssetsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "media-assets")
{
    public Task<HttpResponseMessage> RegisterAsync(
        string? storageProvider = "Uploadcare",
        string storageObjectId = "uploadcare-object",
        string deliveryUrl = "https://ucarecdn.example/uploadcare-object/",
        string? fileName = "video.mp4",
        string contentType = "video/mp4",
        long sizeBytes = 1024,
        string? kind = "Video",
        int? durationSeconds = 12,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            "/api/media/assets",
            new
            {
                storageProvider,
                storageObjectId,
                deliveryUrl,
                fileName,
                contentType,
                sizeBytes,
                kind,
                durationSeconds
            },
            cancellationToken,
            includeCsrfHeader);
    }
}
