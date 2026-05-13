namespace FitLead.IntegrationTests.Clients;

public sealed class TrainerClientsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "trainer-clients")
{
    public Task<HttpResponseMessage> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/trainer/clients", cancellationToken);
    }
}
