using FitLead.Api.TrainingPrograms.Contracts;

namespace FitLead.IntegrationTests.Clients;

public sealed class TrainingProgramsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "training-programs")
{
    public Task<HttpResponseMessage> CreateAsync(
        string title = "Beginner Full Body",
        int weeksCount = 4,
        int daysPerWeek = 7,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            "/api/training-programs",
            new CreateTrainingProgramRequest(title, weeksCount, daysPerWeek),
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> GetAsync(CancellationToken cancellationToken = default)
    {
        return SendGetAsync("/api/training-programs", cancellationToken);
    }

    public Task<HttpResponseMessage> GetByIdAsync(
        Guid programId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/training-programs/{programId:D}", cancellationToken);
    }

    public Task<HttpResponseMessage> GetWorkoutsAsync(
        Guid programId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/training-programs/{programId:D}/workouts", cancellationToken);
    }

    public Task<HttpResponseMessage> AddWorkoutAsync(
        Guid programId,
        Guid workoutId,
        int weekNumber = 1,
        int dayNumber = 1,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            $"/api/training-programs/{programId:D}/workouts",
            new AddWorkoutToProgramRequest(workoutId, weekNumber, dayNumber),
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> ReorderDayAsync(
        Guid programId,
        int weekNumber,
        int dayNumber,
        IReadOnlyList<Guid> entryIds,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Put,
            $"/api/training-programs/{programId:D}/workouts/order",
            new ReorderProgramWorkoutsRequest(weekNumber, dayNumber, entryIds),
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> MoveWorkoutAsync(
        Guid programId,
        Guid trainingProgramWorkoutId,
        int targetWeekNumber,
        int targetDayNumber,
        int targetOrderInDay,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Put,
            $"/api/training-programs/{programId:D}/workouts/{trainingProgramWorkoutId:D}/position",
            new MoveWorkoutEntryRequest(targetWeekNumber, targetDayNumber, targetOrderInDay),
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> RemoveWorkoutAsync(
        Guid programId,
        Guid trainingProgramWorkoutId,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeAsync(
            HttpMethod.Delete,
            $"/api/training-programs/{programId:D}/workouts/{trainingProgramWorkoutId:D}",
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> AssignToClientAsync(
        Guid programId,
        Guid clientId,
        DateTime? expiresAtUtc = null,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            $"/api/training-programs/{programId:D}/assignments",
            new AssignTrainingProgramToClientRequest(clientId, expiresAtUtc),
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> GetAssignmentsAsync(
        Guid programId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync(
            $"/api/training-programs/{programId:D}/assignments",
            cancellationToken);
    }

    public Task<HttpResponseMessage> RevokeAssignmentAsync(
        Guid programId,
        Guid assignmentId,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeAsync(
            HttpMethod.Post,
            $"/api/training-programs/{programId:D}/assignments/{assignmentId:D}/revoke",
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> DeleteAsync(
        Guid programId,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeAsync(
            HttpMethod.Delete,
            $"/api/training-programs/{programId:D}",
            cancellationToken,
            includeCsrfHeader);
    }

    public Task<HttpResponseMessage> ConfirmDeleteAsync(
        Guid programId,
        string token,
        CancellationToken cancellationToken = default,
        bool includeCsrfHeader = true)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            $"/api/training-programs/{programId:D}/deletion-confirmations",
            new ConfirmDeleteTrainingProgramRequest(token),
            cancellationToken,
            includeCsrfHeader);
    }
}
