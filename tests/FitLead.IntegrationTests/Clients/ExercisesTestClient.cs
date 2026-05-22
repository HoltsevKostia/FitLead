using FitLead.Api.Exercises.Contracts;
using FitLead.Domain.Trainings.Exercises;

namespace FitLead.IntegrationTests.Clients;

public sealed class ExercisesTestClient(HttpClient httpClient)
    : AuthenticatedApiTestClient(httpClient, "exercises")
{
    public Task<HttpResponseMessage> CreateAsync(
        string name = "Нова вправа",
        string description = "Опис нової вправи",
        Guid? mediaAssetId = null,
        MuscleGroup? muscleGroup = MuscleGroup.Core,
        Equipment? equipment = Equipment.Bodyweight,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            "/api/exercises",
            new CreateExerciseRequest(name, description, mediaAssetId, muscleGroup, equipment),
            cancellationToken);
    }

    public Task<HttpResponseMessage> UpdateAsync(
        Guid exerciseId,
        string name = "Оновлена вправа",
        string description = "Оновлений опис",
        Guid? mediaAssetId = null,
        MuscleGroup? muscleGroup = MuscleGroup.Core,
        Equipment? equipment = Equipment.Bodyweight,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Put,
            $"/api/exercises/{exerciseId:D}",
            new UpdateExerciseRequest(name, description, mediaAssetId, muscleGroup, equipment),
            cancellationToken);
    }

    public Task<HttpResponseMessage> CopyToMyLibraryAsync(
        Guid exerciseId,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeAsync(
            HttpMethod.Post,
            $"/api/exercises/{exerciseId:D}/copy-to-my-library",
            cancellationToken);
    }

    public Task<HttpResponseMessage> DeleteAsync(
        Guid exerciseId,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeAsync(
            HttpMethod.Delete,
            $"/api/exercises/{exerciseId:D}",
            cancellationToken);
    }

    public Task<HttpResponseMessage> ConfirmDeleteAsync(
        Guid exerciseId,
        string token,
        CancellationToken cancellationToken = default)
    {
        return SendUnsafeJsonAsync(
            HttpMethod.Post,
            $"/api/exercises/{exerciseId:D}/deletion-confirmations",
            new ConfirmDeleteExerciseRequest(token),
            cancellationToken);
    }
}
