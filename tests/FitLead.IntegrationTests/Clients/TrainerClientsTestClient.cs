namespace FitLead.IntegrationTests.Clients;

public sealed class TrainerClientsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "trainer-clients")
{
    public Task<HttpResponseMessage> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/trainer/clients", cancellationToken);
    }

    public Task<HttpResponseMessage> GetWorkspaceAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/trainer/clients/{clientId}/workspace", cancellationToken);
    }

    public Task<HttpResponseMessage> GetOverviewSummaryAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/trainer/clients/{clientId}/overview", cancellationToken);
    }

    public Task<HttpResponseMessage> GetProgramsAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/trainer/clients/{clientId}/programs", cancellationToken);
    }
}
