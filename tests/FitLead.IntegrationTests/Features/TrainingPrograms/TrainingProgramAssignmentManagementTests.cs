using System.Net;
using FitLead.Application.Trainings.TrainingProgramAssignments.Commands;
using FitLead.Application.Trainings.TrainingProgramAssignments.Queries;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainingProgramAssignmentManagementTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task GetAssignments_ShouldReturnProgramAssignmentsForOwner()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-assign-list-trainer");
        var client = await Users.RegisterClientAsync("tp-assign-list-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);
        var assignResponse = await trainerPrograms.AssignToClientAsync(programId, client.Id);
        var created = await assignResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();

        var response = await trainerPrograms.GetAssignmentsAsync(programId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignments = await response.ReadRequiredJsonAsync<IReadOnlyList<TrainingProgramAssignmentDto>>();
        var assignment = assignments.Should().ContainSingle().Subject;
        assignment.AssignmentId.Should().Be(created.AssignmentId);
        assignment.ClientId.Should().Be(client.Id);
        assignment.ClientName.Should().Be("Test Client");
        assignment.Status.Should().Be(nameof(AssignedProgramStatus.Active));
        assignment.AccessSource.Should().Be(nameof(ProgramAccessSource.Manual));
        assignment.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task GetAssignments_ForForeignProgram_ShouldReturnNotFound()
    {
        var ownerTrainer = await Users.RegisterTrainerAsync("tp-assign-list-owner");
        var otherTrainer = await Users.RegisterTrainerAsync("tp-assign-list-other");
        var client = await Users.RegisterClientAsync("tp-assign-list-other-client");
        await CreateTrainerClientRelationshipAsync(ownerTrainer.Id, client.Id);

        var ownerPrograms = await Api.TrainingProgramsAsync(ownerTrainer.Auth);
        var otherPrograms = await Api.TrainingProgramsAsync(otherTrainer.Auth);
        var programId = await CreateProgramAsync(ownerPrograms);
        (await ownerPrograms.AssignToClientAsync(programId, client.Id)).StatusCode
            .Should().Be(HttpStatusCode.Created);

        var response = await otherPrograms.GetAssignmentsAsync(programId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("training_program.not_found");
    }

    [Fact]
    public async Task RevokeAssignment_ByOwner_ShouldMarkAssignmentRevoked()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-assign-revoke-trainer");
        var client = await Users.RegisterClientAsync("tp-assign-revoke-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);
        var assignResponse = await trainerPrograms.AssignToClientAsync(programId, client.Id);
        var created = await assignResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();

        var response = await trainerPrograms.RevokeAssignmentAsync(programId, created.AssignmentId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var assignment = await Db.QueryAsync(context =>
            context.AssignedTrainingPrograms.SingleAsync(x => x.Id == created.AssignmentId));
        assignment.Status.Should().Be(AssignedProgramStatus.Revoked);
        assignment.RevokedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeAssignment_ByDifferentTrainer_ShouldReturnNotFound()
    {
        var ownerTrainer = await Users.RegisterTrainerAsync("tp-assign-revoke-owner");
        var otherTrainer = await Users.RegisterTrainerAsync("tp-assign-revoke-other");
        var client = await Users.RegisterClientAsync("tp-assign-revoke-other-client");
        await CreateTrainerClientRelationshipAsync(ownerTrainer.Id, client.Id);

        var ownerPrograms = await Api.TrainingProgramsAsync(ownerTrainer.Auth);
        var otherPrograms = await Api.TrainingProgramsAsync(otherTrainer.Auth);
        var programId = await CreateProgramAsync(ownerPrograms);
        var assignResponse = await ownerPrograms.AssignToClientAsync(programId, client.Id);
        var created = await assignResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();

        var response = await otherPrograms.RevokeAssignmentAsync(programId, created.AssignmentId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("training_program.not_found");
    }

    [Fact]
    public async Task RevokeAssignment_WithoutCsrf_ShouldReturnBadRequest()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-assign-revoke-csrf-trainer");
        var client = await Users.RegisterClientAsync("tp-assign-revoke-csrf-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);
        var assignResponse = await trainerPrograms.AssignToClientAsync(programId, client.Id);
        var created = await assignResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();

        var response = await trainerPrograms.RevokeAssignmentAsync(
            programId,
            created.AssignmentId,
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignAfterRevoke_ShouldCreateNewActiveAssignment()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-assign-after-revoke-trainer");
        var client = await Users.RegisterClientAsync("tp-assign-after-revoke-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);
        var firstAssignResponse = await trainerPrograms.AssignToClientAsync(programId, client.Id);
        var first = await firstAssignResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();
        (await trainerPrograms.RevokeAssignmentAsync(programId, first.AssignmentId)).StatusCode
            .Should().Be(HttpStatusCode.NoContent);

        var secondAssignResponse = await trainerPrograms.AssignToClientAsync(programId, client.Id);

        secondAssignResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var second = await secondAssignResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();
        second.AssignmentId.Should().NotBe(first.AssignmentId);

        var assignments = await Db.QueryAsync(context =>
            context.AssignedTrainingPrograms
                .Where(x => x.ClientId == client.Id && x.TrainingProgramId == programId)
                .ToListAsync());

        assignments.Should().ContainSingle(x =>
            x.Id == first.AssignmentId &&
            x.Status == AssignedProgramStatus.Revoked);
        assignments.Should().ContainSingle(x =>
            x.Id == second.AssignmentId &&
            x.Status == AssignedProgramStatus.Active);
    }
}
