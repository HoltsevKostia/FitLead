using System.Net;
using FitLead.Application.Trainings.TrainingProgramAssignments.Commands;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Infrastructure.Persistence.Models;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainingProgramAssignmentTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task AssignOwnProgramToOwnClient_ShouldCreateManualActiveAssignment()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-assign-trainer");
        var client = await Users.RegisterClientAsync("tp-assign-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);

        var response = await trainerPrograms.AssignToClientAsync(programId, client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();
        created.ProgramId.Should().Be(programId);
        created.ClientId.Should().Be(client.Id);
        created.Status.Should().Be(nameof(AssignedProgramStatus.Active));
        created.AccessSource.Should().Be(nameof(ProgramAccessSource.Manual));
        created.ExpiresAtUtc.Should().BeNull();

        var assignment = await Db.QueryAsync(context =>
            context.AssignedTrainingPrograms.SingleAsync());
        assignment.Id.Should().Be(created.AssignmentId);
        assignment.TrainerId.Should().Be(trainer.Id);
        assignment.ClientId.Should().Be(client.Id);
        assignment.TrainingProgramId.Should().Be(programId);
        assignment.Status.Should().Be(AssignedProgramStatus.Active);
        assignment.AccessSource.Should().Be(ProgramAccessSource.Manual);
        assignment.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task AssignForeignProgram_ShouldReturnNotFound()
    {
        var ownerTrainer = await Users.RegisterTrainerAsync("tp-assign-owner");
        var otherTrainer = await Users.RegisterTrainerAsync("tp-assign-other");
        var client = await Users.RegisterClientAsync("tp-assign-other-client");
        await CreateTrainerClientRelationshipAsync(otherTrainer.Id, client.Id);

        var ownerPrograms = await Api.TrainingProgramsAsync(ownerTrainer.Auth);
        var otherPrograms = await Api.TrainingProgramsAsync(otherTrainer.Auth);
        var programId = await CreateProgramAsync(ownerPrograms);

        var response = await otherPrograms.AssignToClientAsync(programId, client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("training_program.not_found");
    }

    [Fact]
    public async Task AssignToUnrelatedClient_ShouldReturnNotFound()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-assign-unrelated-trainer");
        var client = await Users.RegisterClientAsync("tp-assign-unrelated-client");
        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);

        var response = await trainerPrograms.AssignToClientAsync(programId, client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("client.not_found");
    }

    [Fact]
    public async Task AssignDuplicateAccessibleAssignment_ShouldReturnConflict()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-assign-duplicate-trainer");
        var client = await Users.RegisterClientAsync("tp-assign-duplicate-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);

        (await trainerPrograms.AssignToClientAsync(programId, client.Id)).StatusCode
            .Should().Be(HttpStatusCode.Created);
        var duplicate = await trainerPrograms.AssignToClientAsync(programId, client.Id);

        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await duplicate.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("assignment.already_exists");
    }

    [Fact]
    public async Task AssignWithPastExpiresAt_ShouldReturnValidationError()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-assign-past-expiry-trainer");
        var client = await Users.RegisterClientAsync("tp-assign-past-expiry-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);

        var response = await trainerPrograms.AssignToClientAsync(
            programId,
            client.Id,
            DateTime.UtcNow.AddDays(-1));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("training_program.assignment.create.expires_at_invalid");
    }

    [Fact]
    public async Task AssignWhenExistingActiveAssignmentExpired_ShouldExpireOldAndCreateNewActive()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-assign-expired-trainer");
        var client = await Users.RegisterClientAsync("tp-assign-expired-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);
        var assignedAtUtc = DateTime.UtcNow.AddDays(-3);
        var expiresAtUtc = DateTime.UtcNow.AddDays(-1);
        var oldAssignment = AssignedTrainingProgram.AssignManually(
            trainer.Id,
            client.Id,
            programId,
            assignedAtUtc,
            expiresAtUtc);
        oldAssignment.IsSuccess.Should().BeTrue();

        await Db.ExecuteAsync(async context =>
        {
            await context.AssignedTrainingPrograms.AddAsync(oldAssignment.Value);
            await context.SaveChangesAsync();
        });

        var response = await trainerPrograms.AssignToClientAsync(programId, client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();
        created.AssignmentId.Should().NotBe(oldAssignment.Value.Id);

        var assignments = await Db.QueryAsync(context =>
            context.AssignedTrainingPrograms
                .Where(x => x.ClientId == client.Id && x.TrainingProgramId == programId)
                .OrderBy(x => x.AssignedAtUtc)
                .ToListAsync());

        assignments.Should().HaveCount(2);
        assignments.Should().ContainSingle(x =>
            x.Id == oldAssignment.Value.Id &&
            x.Status == AssignedProgramStatus.Expired);
        assignments.Should().ContainSingle(x =>
            x.Id == created.AssignmentId &&
            x.Status == AssignedProgramStatus.Active);
    }

    [Fact]
    public async Task AssignWithoutCsrf_ShouldReturnBadRequest()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-assign-csrf-trainer");
        var client = await Users.RegisterClientAsync("tp-assign-csrf-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);

        var response = await trainerPrograms.AssignToClientAsync(
            programId,
            client.Id,
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task CreateTrainerClientRelationshipAsync(Guid trainerId, Guid clientId)
    {
        await Db.ExecuteAsync(async context =>
        {
            await context.TrainerClients.AddAsync(new TrainerClient(trainerId, clientId));
            await context.SaveChangesAsync();
        });
    }
}
