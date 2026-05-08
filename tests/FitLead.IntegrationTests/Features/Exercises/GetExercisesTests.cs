using System.Net;
using FitLead.Application.Trainings.Exercises.Queries;
using FitLead.Domain.Trainings;
using FitLead.Infrastructure.Persistence;
using FitLead.Infrastructure.Persistence.Seeding;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Exercises;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class GetExercisesTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetExercises_WithSourceFilters_ShouldReturnOnlyPlatformAndOwnExercises()
    {
        var trainerAuth = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var otherTrainerAuth = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var trainerEmail = UniqueEmail("exercise-list-trainer");
        var otherTrainerEmail = UniqueEmail("exercise-list-other");

        var trainerRegister = await trainerAuth.RegisterAsync(
            trainerEmail,
            "Str0ngPass!123",
            "Exercise List Trainer",
            AuthRoles.Trainer);
        trainerRegister.StatusCode.Should().Be(HttpStatusCode.Created);

        var otherTrainerRegister = await otherTrainerAuth.RegisterAsync(
            otherTrainerEmail,
            "Str0ngPass!123",
            "Other Exercise Trainer",
            AuthRoles.Trainer);
        otherTrainerRegister.StatusCode.Should().Be(HttpStatusCode.Created);

        await using (var scope = Fixture.Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
            await PlatformExerciseSeeder.SeedAsync(dbContext);

            var trainerId = await GetDomainUserIdAsync(dbContext, trainerEmail);
            var otherTrainerId = await GetDomainUserIdAsync(dbContext, otherTrainerEmail);

            dbContext.Exercises.Add(Exercise.CreateTrainerExercise(
                trainerId,
                "Моя тестова вправа",
                "Опис власної вправи",
                muscleGroup: MuscleGroup.Core,
                equipment: Equipment.Bodyweight).Value);

            dbContext.Exercises.Add(Exercise.CreateTrainerExercise(
                otherTrainerId,
                "Чужа тестова вправа",
                "Опис чужої вправи",
                muscleGroup: MuscleGroup.Back,
                equipment: Equipment.Dumbbells).Value);

            await dbContext.SaveChangesAsync();
        }

        var allResponse = await trainerAuth.GetAsync("/api/exercises");
        var explicitAllResponse = await trainerAuth.GetAsync("/api/exercises?source=all");
        var platformResponse = await trainerAuth.GetAsync("/api/exercises?source=platform");
        var myResponse = await trainerAuth.GetAsync("/api/exercises?source=my");

        allResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        explicitAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        platformResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        myResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var all = await allResponse.ReadRequiredJsonAsync<List<ExerciseDto>>();
        var explicitAll = await explicitAllResponse.ReadRequiredJsonAsync<List<ExerciseDto>>();
        var platform = await platformResponse.ReadRequiredJsonAsync<List<ExerciseDto>>();
        var my = await myResponse.ReadRequiredJsonAsync<List<ExerciseDto>>();

        all.Should().Contain(x => x.Source == ExerciseSource.Platform && !x.IsEditable);
        all.Should().ContainSingle(x =>
            x.Name == "Моя тестова вправа" &&
            x.Source == ExerciseSource.Trainer &&
            x.IsEditable);
        all.Should().NotContain(x => x.Name == "Чужа тестова вправа");

        explicitAll.Select(x => x.Id).Should().BeEquivalentTo(all.Select(x => x.Id));

        platform.Should().HaveCount(PlatformExerciseSeeder.Exercises.Count);
        platform.Should().OnlyContain(x =>
            x.Source == ExerciseSource.Platform &&
            !x.IsEditable);

        my.Should().ContainSingle(x =>
            x.Name == "Моя тестова вправа" &&
            x.Source == ExerciseSource.Trainer &&
            x.IsEditable);
        my.Should().OnlyContain(x => x.Source == ExerciseSource.Trainer);
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
