using System.Net;
using System.Text.Json;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Trainings.TrainingProgramAssignments.Commands;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

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

        var outboxMessage = await Db.QueryAsync(async context =>
        {
            var messages = await context.OutboxMessages
                .AsNoTracking()
                .Where(message => message.Type == OutboxEventTypes.Training.ProgramAssigned)
                .ToListAsync();

            return messages.Single(message =>
                HasProgramAssignedPayload(
                    message.Payload,
                    assignment.AssignmentId,
                    programId,
                    trainer.Id,
                    client.Id,
                    "Strength base"));
        });

        outboxMessage.Status.Should().NotBe(OutboxMessageStatus.Failed);
    }

    private static bool HasProgramAssignedPayload(
        string payload,
        Guid expectedAssignmentId,
        Guid expectedProgramId,
        Guid expectedTrainerId,
        Guid expectedClientId,
        string expectedProgramTitle)
    {
        using var document = JsonDocument.Parse(payload);

        return document.RootElement.GetProperty("assignmentId").GetGuid() == expectedAssignmentId &&
               document.RootElement.GetProperty("trainingProgramId").GetGuid() == expectedProgramId &&
               document.RootElement.GetProperty("trainerId").GetGuid() == expectedTrainerId &&
               document.RootElement.GetProperty("clientId").GetGuid() == expectedClientId &&
               document.RootElement.GetProperty("programTitle").GetString() == expectedProgramTitle;
    }
}
