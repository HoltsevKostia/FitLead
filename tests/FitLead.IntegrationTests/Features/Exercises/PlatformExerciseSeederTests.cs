using FitLead.Domain.Trainings;
using FitLead.Infrastructure.Persistence;
using FitLead.Infrastructure.Persistence.Seeding;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Exercises;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class PlatformExerciseSeederTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SeedAsync_WhenRunTwice_ShouldCreatePlatformExercisesWithoutDuplicates()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

        await PlatformExerciseSeeder.SeedAsync(dbContext);

        var firstRunExercises = await dbContext.Exercises
            .AsNoTracking()
            .Where(x => x.Source == ExerciseSource.Platform)
            .ToListAsync();

        await PlatformExerciseSeeder.SeedAsync(dbContext);

        var secondRunExercises = await dbContext.Exercises
            .AsNoTracking()
            .Where(x => x.Source == ExerciseSource.Platform)
            .ToListAsync();

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
}
