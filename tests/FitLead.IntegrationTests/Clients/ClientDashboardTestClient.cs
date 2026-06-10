namespace FitLead.IntegrationTests.Clients;

public sealed class ClientDashboardTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "client-dashboard")
{
    public Task<HttpResponseMessage> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/client/dashboard", cancellationToken);
    }
}
