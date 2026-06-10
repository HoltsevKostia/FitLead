namespace FitLead.IntegrationTests.Clients;

public sealed class TrainerVideoReportsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "trainer-video-reports")
{
    public Task<HttpResponseMessage> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/trainer/video-reports/pending", cancellationToken);
    }
}
