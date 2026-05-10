using FitLead.Domain.Trainings.Exercises;
using FitLead.Infrastructure.Persistence.Seeding;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.Exercises;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class PlatformExerciseSeederTests : IntegrationTestBase
{
    private readonly TestDb _db;

    public PlatformExerciseSeederTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _db = new TestDb(fixture);
    }

    [Fact]
    public async Task SeedAsync_WhenRunTwice_ShouldCreatePlatformExercisesWithoutDuplicates()
    {
        await _db.ExecuteAsync(context => PlatformExerciseSeeder.SeedAsync(context));

        var firstRunExercises = await GetPlatformExercisesAsync();

        await _db.ExecuteAsync(context => PlatformExerciseSeeder.SeedAsync(context));

        var secondRunExercises = await GetPlatformExercisesAsync();

        firstRunExercises.Should().HaveCount(PlatformExerciseSeeder.Exercises.Count);
        secondRunExercises.Should().HaveCount(firstRunExercises.Count);
        secondRunExercises.Select(x => x.Name).Should().OnlyHaveUniqueItems();
        secondRunExercises.Should().OnlyContain(x =>
            x.Source == ExerciseSource.Platform &&
            x.OwnerTrainerId == null &&
            x.CopiedFromExerciseId == null &&
            x.MuscleGroup != null &&
            x.Equipment != null);
    }

    private Task<List<Exercise>> GetPlatformExercisesAsync()
    {
        return _db.QueryAsync(context =>
            context.Exercises
                .AsNoTracking()
                .Where(x => x.Source == ExerciseSource.Platform)
                .ToListAsync());
    }
}
