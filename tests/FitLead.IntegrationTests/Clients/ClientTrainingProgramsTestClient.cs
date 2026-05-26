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

    public Task<HttpResponseMessage> UpsertWorkoutLogAsync(
        Guid assignmentId,
        Guid programWorkoutId,
        string status,
        DateTime? performedAtUtc = null,
        string? clientNote = null,
        int? difficultyRating = null,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Put,
            $"/api/client/training-program-assignments/{assignmentId:D}/workouts/{programWorkoutId:D}/log",
            new
            {
                status,
                performedAtUtc,
                clientNote,
                difficultyRating
            },
            cancellationToken,
            includeCsrfHeader);
    }
}
