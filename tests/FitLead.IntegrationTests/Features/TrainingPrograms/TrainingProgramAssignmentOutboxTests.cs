using System.Net;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Trainings.TrainingProgramAssignments.Commands;
using FitLead.Application.Trainings.TrainingProgramAssignments.Outbox;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainingProgramAssignmentOutboxTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task AssignProgram_ShouldCreateProgramAssignedOutboxMessage()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-assign-outbox-trainer");
        var client = await Users.RegisterClientAsync("tp-assign-outbox-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms, "Strength base");

        var response = await trainerPrograms.AssignToClientAsync(programId, client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var assignment = await response.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();

        var outboxMessage = await Outbox.GetSingleAsync<TrainingProgramAssignedOutboxPayload>(
            OutboxEventTypes.Training.ProgramAssigned,
            payload => payload.AssignmentId == assignment.AssignmentId &&
                       payload.TrainingProgramId == programId &&
                       payload.TrainerId == trainer.Id &&
                       payload.ClientId == client.Id &&
                       payload.ProgramTitle == "Strength base");

        outboxMessage.Status.Should().NotBe(OutboxMessageStatus.Failed);
    }
}
