using System.Net;
using FitLead.Domain.Trainings;
using FitLead.Infrastructure.Persistence;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Exercises;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class CopyExerciseToMyLibraryTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task CopyToMyLibrary_WithPlatformExercise_ShouldCreateEditableTrainerCopy()
    {
        var trainer = await RegisterTrainerAsync("copy-platform");
        var platformExerciseId = await AddPlatformExerciseAsync();
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var copyResponse = await client.CopyToMyLibraryAsync(platformExerciseId);

        copyResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var copiedExerciseId = await copyResponse.ReadRequiredJsonAsync<Guid>();

        await using (var scope = Fixture.Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
            var trainerId = await GetDomainUserIdAsync(dbContext, trainer.Email);
            var copiedExercise = await dbContext.Exercises.SingleAsync(x => x.Id == copiedExerciseId);

            copiedExercise.Source.Should().Be(ExerciseSource.Trainer);
            copiedExercise.OwnerTrainerId.Should().Be(trainerId);
            copiedExercise.CopiedFromExerciseId.Should().Be(platformExerciseId);
            copiedExercise.MuscleGroup.Should().Be(MuscleGroup.Chest);
            copiedExercise.Equipment.Should().Be(Equipment.Barbell);
        }

        var updateResponse = await client.UpdateAsync(
            copiedExerciseId,
            name: "Моя адаптована вправа",
            description: "Опис адаптованої вправи");

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CopyToMyLibrary_WithOwnTrainerExercise_ShouldReturnBadRequest()
    {
        var trainer = await RegisterTrainerAsync("copy-own");
        var exerciseId = await AddTrainerExerciseAsync(trainer.Email);
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var response = await client.CopyToMyLibraryAsync(exerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.copy.source_must_be_platform");
    }

    [Fact]
    public async Task CopyToMyLibrary_WithAnotherTrainerExercise_ShouldReturnNotFound()
    {
        var trainer = await RegisterTrainerAsync("copy-other-current");
        var otherTrainer = await RegisterTrainerAsync("copy-other-owner");
        var otherExerciseId = await AddTrainerExerciseAsync(otherTrainer.Email);
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var response = await client.CopyToMyLibraryAsync(otherExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    [Fact]
    public async Task CopyToMyLibrary_WhenCopyAlreadyExists_ShouldReturnConflict()
    {
        var trainer = await RegisterTrainerAsync("copy-duplicate");
        var platformExerciseId = await AddPlatformExerciseAsync();
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var firstResponse = await client.CopyToMyLibraryAsync(platformExerciseId);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await client.CopyToMyLibraryAsync(platformExerciseId);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await secondResponse.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.copy.already_exists");
    }

    private async Task<(AuthTestClient Auth, string Email)> RegisterTrainerAsync(string prefix)
    {
        var auth = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail(prefix);

        var response = await auth.RegisterAsync(
            email,
            "Str0ngPass!123",
            "Copy Exercise Trainer",
            AuthRoles.Trainer);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (auth, email);
    }

    private async Task<ExercisesTestClient> CreateExercisesClientAsync(AuthTestClient auth)
    {
        var client = new ExercisesTestClient(Fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    private async Task<Guid> AddPlatformExerciseAsync()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

        var exercise = Exercise.CreatePlatformExercise(
            $"Платформна вправа {Guid.NewGuid():N}",
            "Опис платформної вправи",
            muscleGroup: MuscleGroup.Chest,
            equipment: Equipment.Barbell).Value;

        dbContext.Exercises.Add(exercise);
        await dbContext.SaveChangesAsync();

        return exercise.Id;
    }

    private async Task<Guid> AddTrainerExerciseAsync(string trainerEmail)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        var trainerId = await GetDomainUserIdAsync(dbContext, trainerEmail);

        var exercise = Exercise.CreateTrainerExercise(
            trainerId,
            $"Власна вправа {Guid.NewGuid():N}",
            "Опис власної вправи").Value;

        dbContext.Exercises.Add(exercise);
        await dbContext.SaveChangesAsync();

        return exercise.Id;
    }

    private static async Task<Guid> GetDomainUserIdAsync(
        FitLeadDbContext dbContext,
        string email)
    {
        return await dbContext.DomainUsers
            .Where(x => x.Email == email)
            .Select(x => x.Id)
            .SingleAsync();
    }
}
