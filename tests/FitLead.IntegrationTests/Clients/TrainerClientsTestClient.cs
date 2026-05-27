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

    public Task<HttpResponseMessage> GetWorkoutLogsAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/trainer/clients/{clientId}/workout-logs", cancellationToken);
    }

    public Task<HttpResponseMessage> GetProgressAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/trainer/clients/{clientId}/progress", cancellationToken);
    }

    public Task<HttpResponseMessage> GetVideoReportsAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/trainer/clients/{clientId}/video-reports", cancellationToken);
    }

    public Task<HttpResponseMessage> GetProfileAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/trainer/clients/{clientId}/profile", cancellationToken);
    }
}
