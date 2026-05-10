using System.Net;
using FitLead.Application.Trainings.Exercises.Queries;
using FitLead.Domain.Trainings.Exercises;
using FitLead.Infrastructure.Persistence.Seeding;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Exercises;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class GetExercisesTests : IntegrationTestBase
{
    private readonly TestDb _db;
    private readonly TestUsers _users;
    private readonly TestExercises _exercises;

    public GetExercisesTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _db = new TestDb(fixture);
        _users = new TestUsers(fixture, _db);
        _exercises = new TestExercises(_db);
    }

    [Fact]
    public async Task GetExercises_WithSourceFilters_ShouldReturnOnlyPlatformAndOwnExercises()
    {
        var trainer = await _users.RegisterTrainerAsync("exercise-list-trainer");
        var otherTrainer = await _users.RegisterTrainerAsync("exercise-list-other");

        await _db.ExecuteAsync(context => PlatformExerciseSeeder.SeedAsync(context));
        await _exercises.CreateTrainerExerciseAsync(
            trainer.Id,
            "Моя тестова вправа",
            "Опис власної вправи",
            MuscleGroup.Core,
            Equipment.Bodyweight);
        await _exercises.CreateTrainerExerciseAsync(
            otherTrainer.Id,
            "Чужа тестова вправа",
            "Опис чужої вправи",
            MuscleGroup.Back,
            Equipment.Dumbbells);

        var allResponse = await trainer.Auth.GetAsync("/api/exercises");
        var explicitAllResponse = await trainer.Auth.GetAsync("/api/exercises?source=all");
        var platformResponse = await trainer.Auth.GetAsync("/api/exercises?source=platform");
        var myResponse = await trainer.Auth.GetAsync("/api/exercises?source=my");

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
}
