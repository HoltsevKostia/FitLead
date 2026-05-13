using System.Net;
using FitLead.Application.Users.Queries;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainerClientsOverviewTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task GetOverview_ShouldReturnTrainerClientsWithAccessiblePrograms()
    {
        var trainer = await Users.RegisterTrainerAsync("trainer-clients-overview-trainer");
        var client = await Users.RegisterClientAsync("trainer-clients-overview-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainingPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainingPrograms, "Strength Base");
        var assignResponse = await trainingPrograms.AssignToClientAsync(
            programId,
            client.Id,
            DateTime.UtcNow.AddDays(10));
        assignResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var trainerClients = await Api.TrainerClientsAsync(trainer.Auth);
        var response = await trainerClients.GetOverviewAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var overview = await response.ReadRequiredJsonAsync<IReadOnlyList<TrainerClientOverviewDto>>();
        var clientOverview = overview.Should().ContainSingle(x => x.ClientId == client.Id).Subject;
        clientOverview.Email.Should().Be(client.Email);
        clientOverview.ActivePrograms.Should().ContainSingle(program =>
            program.ProgramId == programId &&
            program.ProgramTitle == "Strength Base" &&
            program.ExpiresAtUtc.HasValue);
    }

    [Fact]
    public async Task GetOverview_ShouldNotReturnRevokedOrExpiredPrograms()
    {
        var trainer = await Users.RegisterTrainerAsync("trainer-clients-hidden-trainer");
        var client = await Users.RegisterClientAsync("trainer-clients-hidden-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainingPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var revokedProgramId = await CreateProgramAsync(trainingPrograms, "Revoked Program");
        var expiredProgramId = await CreateProgramAsync(trainingPrograms, "Expired Program");

        var revokedResponse = await trainingPrograms.AssignToClientAsync(revokedProgramId, client.Id);
        revokedResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var revokedAssignmentId = await Db.QueryAsync(context =>
            context.AssignedTrainingPrograms
                .Where(x => x.TrainingProgramId == revokedProgramId)
                .Select(x => x.Id)
                .SingleAsync());
        (await trainingPrograms.RevokeAssignmentAsync(revokedProgramId, revokedAssignmentId))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var expiredAssignment = AssignedTrainingProgram.AssignManually(
            trainer.Id,
            client.Id,
            expiredProgramId,
            DateTime.UtcNow.AddDays(-3),
            DateTime.UtcNow.AddDays(-1));
        expiredAssignment.IsSuccess.Should().BeTrue();
        await Db.ExecuteAsync(async context =>
        {
            await context.AssignedTrainingPrograms.AddAsync(expiredAssignment.Value);
            await context.SaveChangesAsync();
        });

        var trainerClients = await Api.TrainerClientsAsync(trainer.Auth);
        var response = await trainerClients.GetOverviewAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var overview = await response.ReadRequiredJsonAsync<IReadOnlyList<TrainerClientOverviewDto>>();
        var clientOverview = overview.Should().ContainSingle(x => x.ClientId == client.Id).Subject;
        clientOverview.ActivePrograms.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOverview_AsClient_ShouldReturnForbidden()
    {
        var clientUser = await Users.RegisterClientAsync("trainer-clients-forbidden-client");
        var trainerClients = await Api.TrainerClientsAsync(clientUser.Auth);

        var response = await trainerClients.GetOverviewAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
