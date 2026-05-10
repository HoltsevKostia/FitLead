using FitLead.Api.Exercises.Contracts;
using FitLead.Api.Workouts.Contracts;

namespace FitLead.IntegrationTests.Clients;

public sealed class WorkoutsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "workouts")
{
    public Task<HttpResponseMessage> AddExerciseAsync(
        Guid workoutId,
        Guid exerciseId,
        int repetitions = 10,
        int sets = 3,
        decimal? loadKg = null,
        int restSeconds = 60,
        string? trainerNote = null,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            $"/api/workouts/{workoutId:D}/exercises",
            new AddExerciseToWorkoutRequest(
                exerciseId,
                repetitions,
                sets,
                loadKg,
                restSeconds,
                trainerNote),
            cancellationToken);
    }

    public Task<HttpResponseMessage> CreateAsync(
        string name = "Тестове тренування",
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            "/api/workouts",
            new CreateWorkoutRequest(name),
            cancellationToken);
    }

    public Task<HttpResponseMessage> GetDetailsAsync(
        Guid workoutId,
        CancellationToken cancellationToken = default)
    {
        return SendGetAsync($"/api/workouts/{workoutId:D}", cancellationToken);
    }

    public Task<HttpResponseMessage> UpdateExerciseAsync(
        Guid workoutId,
        Guid workoutExerciseId,
        int repetitions = 12,
        int sets = 4,
        decimal? loadKg = null,
        int restSeconds = 90,
        string? trainerNote = null,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Put,
            $"/api/workouts/{workoutId:D}/exercises/{workoutExerciseId:D}",
            new UpdateWorkoutExerciseRequest(
                repetitions,
                sets,
                loadKg,
                restSeconds,
                trainerNote),
            cancellationToken);
    }

    public Task<HttpResponseMessage> RemoveExerciseAsync(
        Guid workoutId,
        Guid workoutExerciseId,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeAsync(
            HttpMethod.Delete,
            $"/api/workouts/{workoutId:D}/exercises/{workoutExerciseId:D}",
            cancellationToken);
    }
}
