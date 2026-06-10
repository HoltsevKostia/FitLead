namespace FitLead.IntegrationTests.Clients;

public sealed class TrainerDashboardTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "trainer-dashboard")
{
    public Task<HttpResponseMessage> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/trainer/dashboard", cancellationToken);
    }
}
