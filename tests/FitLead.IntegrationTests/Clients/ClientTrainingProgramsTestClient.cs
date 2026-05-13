namespace FitLead.IntegrationTests.Clients;

public sealed class ClientTrainingProgramsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "client-training-programs")
{
    public Task<HttpResponseMessage> GetAssignedProgramsAsync(
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/client/training-programs", cancellationToken);
    }

    public Task<HttpResponseMessage> GetAssignedProgramDetailsAsync(
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync(
            $"/api/client/training-programs/{assignmentId:D}",
            cancellationToken);
    }
}
