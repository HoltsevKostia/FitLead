using System.Text.Json;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Messenger.VideoReports.Outbox;
using FitLead.Application.Trainings.TrainingProgramAssignments.Outbox;
using FitLead.Domain.Notifications;
using FitLead.Domain.Outbox;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Notifications;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class NotificationOutboxHandlerTests : NotificationTestBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public NotificationOutboxHandlerTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ProcessVideoReportSubmitted_ShouldCreateTrainerNotification()
    {
        var trainer = await Users.RegisterTrainerAsync("notif-outbox-submitted-trainer");
        var client = await Users.RegisterClientAsync("notif-outbox-submitted-client");
        var chatId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var message = await AddOutboxMessageAsync(
            OutboxEventTypes.Messenger.VideoReportSubmitted,
            new VideoReportSubmittedOutboxPayload(
                chatId,
                reportId,
                client.Id,
                trainer.Id,
                "Squat check",
                DateTime.UtcNow));

        await ProcessAsync(message.Id);

        var notification = await GetSingleNotificationAsync();
        notification.RecipientUserId.Should().Be(trainer.Id);
        notification.Type.Should().Be(NotificationType.VideoReportSubmitted);
        notification.Title.Should().Be("Новий відео-звіт");
        notification.Body.Should().Be("Squat check");
        notification.LinkUrl.Should().Be($"/chats/{chatId}/reports/{reportId}");
        notification.SourceEventId.Should().Be(message.Id);
    }

    [Fact]
    public async Task ProcessVideoReportReviewed_ShouldCreateClientNotification()
    {
        var trainer = await Users.RegisterTrainerAsync("notif-outbox-reviewed-trainer");
        var client = await Users.RegisterClientAsync("notif-outbox-reviewed-client");
        var chatId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var message = await AddOutboxMessageAsync(
            OutboxEventTypes.Messenger.VideoReportReviewed,
            new VideoReportReviewedOutboxPayload(
                chatId,
                reportId,
                client.Id,
                trainer.Id,
                "Deadlift check",
                DateTime.UtcNow));

        await ProcessAsync(message.Id);

        var notification = await GetSingleNotificationAsync();
        notification.RecipientUserId.Should().Be(client.Id);
        notification.Type.Should().Be(NotificationType.VideoReportReviewed);
        notification.Title.Should().Be("Відео-звіт переглянуто");
        notification.Body.Should().Be("Deadlift check");
        notification.LinkUrl.Should().Be($"/chats/{chatId}/reports/{reportId}");
        notification.SourceEventId.Should().Be(message.Id);
    }

    [Fact]
    public async Task ProcessTrainingProgramAssigned_ShouldCreateClientNotification()
    {
        var trainer = await Users.RegisterTrainerAsync("notif-outbox-program-trainer");
        var client = await Users.RegisterClientAsync("notif-outbox-program-client");
        var assignmentId = Guid.NewGuid();
        var message = await AddOutboxMessageAsync(
            OutboxEventTypes.Training.ProgramAssigned,
            new TrainingProgramAssignedOutboxPayload(
                assignmentId,
                Guid.NewGuid(),
                trainer.Id,
                client.Id,
                "Strength base",
                DateTime.UtcNow));

        await ProcessAsync(message.Id);

        var notification = await GetSingleNotificationAsync();
        notification.RecipientUserId.Should().Be(client.Id);
        notification.Type.Should().Be(NotificationType.TrainingProgramAssigned);
        notification.Title.Should().Be("Призначено програму тренувань");
        notification.Body.Should().Be("Strength base");
        notification.LinkUrl.Should().Be($"/client/training-programs/{assignmentId}");
        notification.SourceEventId.Should().Be(message.Id);
    }

    [Fact]
    public async Task ProcessAlreadyCreatedNotification_ShouldNotCreateDuplicate()
    {
        var trainer = await Users.RegisterTrainerAsync("notif-outbox-duplicate-trainer");
        var client = await Users.RegisterClientAsync("notif-outbox-duplicate-client");
        var message = await AddOutboxMessageAsync(
            OutboxEventTypes.Messenger.VideoReportSubmitted,
            new VideoReportSubmittedOutboxPayload(
                Guid.NewGuid(),
                Guid.NewGuid(),
                client.Id,
                trainer.Id,
                "Squat check",
                DateTime.UtcNow));
        await CreateNotificationAsync(
            trainer.Id,
            sourceEventId: message.Id);

        await ProcessAsync(message.Id);

        var notifications = await Db.QueryAsync(context =>
            context.Notifications
                .Where(notification => notification.SourceEventId == message.Id)
                .ToListAsync());
        notifications.Should().HaveCount(1);
    }

    private async Task<OutboxMessage> AddOutboxMessageAsync<TPayload>(
        string type,
        TPayload payload)
    {
        var serializedPayload = JsonSerializer.Serialize(payload, SerializerOptions);
        var message = OutboxMessage.Create(
            type,
            serializedPayload,
            DateTime.UtcNow).Value;

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await repository.AddAsync(message, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        return message;
    }

    private async Task ProcessAsync(Guid messageId)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();
        await processor.ProcessAsync(messageId, CancellationToken.None);
    }

    private async Task<Notification> GetSingleNotificationAsync()
    {
        return await Db.QueryAsync(context =>
            context.Notifications.SingleAsync());
    }
}
