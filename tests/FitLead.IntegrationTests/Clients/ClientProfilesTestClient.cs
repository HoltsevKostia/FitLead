namespace FitLead.IntegrationTests.Clients;

public sealed class ClientProfilesTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "client-profiles")
{
    public Task<HttpResponseMessage> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/client/profile", cancellationToken);
    }

    public Task<HttpResponseMessage> UpdateAsync(
        string? goal = null,
        string? experienceLevel = null,
        int? heightCm = null,
        string? limitations = null,
        string? trainingPreferences = null,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Put,
            "/api/client/profile",
            new
            {
                goal,
                experienceLevel,
                heightCm,
                limitations,
                trainingPreferences,
                additionalInfo
            },
            cancellationToken,
            includeCsrfHeader);
    }
}
