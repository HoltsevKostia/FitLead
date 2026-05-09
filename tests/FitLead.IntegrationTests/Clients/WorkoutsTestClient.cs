using FitLead.Api.Exercises.Contracts;

namespace FitLead.IntegrationTests.Clients;

public sealed class WorkoutsTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "workouts")
{
    public Task<HttpResponseMessage> AddExerciseAsync(
        Guid workoutId,
        Guid exerciseId,
        int repetitions = 10,
        int sets = 3,
        int restSeconds = 60,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            $"/api/workouts/{workoutId:D}/exercises",
            new AddExerciseToWorkoutRequest(exerciseId, repetitions, sets, restSeconds),
            cancellationToken);
    }
}
