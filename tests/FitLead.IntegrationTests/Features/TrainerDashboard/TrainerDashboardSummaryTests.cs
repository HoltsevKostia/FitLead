using System.Net;
using FitLead.Application.TrainerDashboard.Queries;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Messenger.VideoReports;
using FitLead.Domain.Notifications;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Domain.Trainings.TrainingPrograms;
using FitLead.Infrastructure.Persistence.Models;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.TrainerDashboard;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainerDashboardSummaryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private readonly TestDb _db = new(fixture);
    private readonly TestUsers _users = new(fixture, new TestDb(fixture));
    private readonly TestApiClients _api = new(fixture);

    [Fact]
    public async Task GetSummary_ShouldReturnOnlyCurrentTrainerCounts()
    {
        var trainer = await _users.RegisterTrainerAsync("dashboard-owner");
        var otherTrainer = await _users.RegisterTrainerAsync("dashboard-other");
        var client = await _users.RegisterClientAsync("dashboard-client");
        var otherClient = await _users.RegisterClientAsync("dashboard-other-client");

        await CreateRelationshipAsync(trainer.Id, client.Id);
        await CreateRelationshipAsync(otherTrainer.Id, otherClient.Id);
        await CreateAssignmentAsync(trainer.Id, client.Id, "Owner active program");
        await CreateAssignmentAsync(otherTrainer.Id, otherClient.Id, "Other active program");
        await CreateNotificationAsync(trainer.Id, isRead: false);
        await CreateNotificationAsync(trainer.Id, isRead: true);
        await CreateNotificationAsync(otherTrainer.Id, isRead: false);
        await CreateVideoReportAsync(trainer.Id, client.Id, reviewed: false);
        await CreateVideoReportAsync(otherTrainer.Id, otherClient.Id, reviewed: false);

        var dashboard = await _api.TrainerDashboardAsync(trainer.Auth);
        var response = await dashboard.GetSummaryAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.ReadRequiredJsonAsync<TrainerDashboardSummaryDto>();
        summary.ClientCount.Should().Be(1);
        summary.ActiveProgramAssignmentCount.Should().Be(1);
        summary.UnreadNotificationCount.Should().Be(1);
        summary.PendingVideoReportCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSummary_ShouldExcludeRevokedAndExpiredAssignments()
    {
        var trainer = await _users.RegisterTrainerAsync("dashboard-programs-trainer");
        var client = await _users.RegisterClientAsync("dashboard-programs-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);

        await CreateAssignmentAsync(trainer.Id, client.Id, "Active program");
        await CreateAssignmentAsync(trainer.Id, client.Id, "Revoked program", revoke: true);
        await CreateAssignmentAsync(
            trainer.Id,
            client.Id,
            "Expired program",
            expiresAtUtc: DateTime.UtcNow.AddDays(-1));

        var dashboard = await _api.TrainerDashboardAsync(trainer.Auth);
        var response = await dashboard.GetSummaryAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.ReadRequiredJsonAsync<TrainerDashboardSummaryDto>();
        summary.ActiveProgramAssignmentCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSummary_ShouldExcludeReviewedVideoReports()
    {
        var trainer = await _users.RegisterTrainerAsync("dashboard-reports-trainer");
        var client = await _users.RegisterClientAsync("dashboard-reports-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        await CreateVideoReportAsync(trainer.Id, client.Id, reviewed: false);
        await CreateVideoReportAsync(trainer.Id, client.Id, reviewed: true);

        var dashboard = await _api.TrainerDashboardAsync(trainer.Auth);
        var response = await dashboard.GetSummaryAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.ReadRequiredJsonAsync<TrainerDashboardSummaryDto>();
        summary.PendingVideoReportCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSummary_AsClient_ShouldReturnForbidden()
    {
        var client = await _users.RegisterClientAsync("dashboard-forbidden-client");
        var dashboard = await _api.TrainerDashboardAsync(client.Auth);

        var response = await dashboard.GetSummaryAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSummary_Unauthenticated_ShouldReturnUnauthorized()
    {
        var response = await HttpClient.GetAsync("/api/trainer/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task CreateRelationshipAsync(Guid trainerId, Guid clientId)
    {
        await _db.ExecuteAsync(async context =>
        {
            await context.TrainerClients.AddAsync(new TrainerClient(trainerId, clientId));
            await context.SaveChangesAsync();
        });
    }

    private async Task CreateAssignmentAsync(
        Guid trainerId,
        Guid clientId,
        string title,
        DateTime? expiresAtUtc = null,
        bool revoke = false)
    {
        var programResult = TrainingProgram.Create(
            trainerId,
            title,
            weeksCount: 4,
            daysPerWeek: 3);
        programResult.IsSuccess.Should().BeTrue();

        var utcNow = DateTime.UtcNow;
        var assignmentResult = AssignedTrainingProgram.AssignManually(
            trainerId,
            clientId,
            programResult.Value.Id,
            utcNow.AddDays(-2),
            expiresAtUtc);
        assignmentResult.IsSuccess.Should().BeTrue();

        if (revoke)
        {
            assignmentResult.Value.Revoke(utcNow.AddDays(-1)).IsSuccess.Should().BeTrue();
        }

        await _db.ExecuteAsync(async context =>
        {
            await context.TrainingPrograms.AddAsync(programResult.Value);
            await context.AssignedTrainingPrograms.AddAsync(assignmentResult.Value);
            await context.SaveChangesAsync();
        });
    }

    private async Task CreateNotificationAsync(Guid recipientUserId, bool isRead)
    {
        var utcNow = DateTime.UtcNow;
        var notificationResult = Notification.Create(
            recipientUserId,
            NotificationType.VideoReportSubmitted,
            "Video report submitted",
            "Please review",
            "/chats",
            utcNow,
            Guid.NewGuid());
        notificationResult.IsSuccess.Should().BeTrue();

        if (isRead)
        {
            notificationResult.Value.MarkRead(utcNow.AddMinutes(1)).IsSuccess.Should().BeTrue();
        }

        await _db.ExecuteAsync(async context =>
        {
            await context.Notifications.AddAsync(notificationResult.Value);
            await context.SaveChangesAsync();
        });
    }

    private async Task CreateVideoReportAsync(
        Guid trainerId,
        Guid clientId,
        bool reviewed)
    {
        var utcNow = DateTime.UtcNow;
        var existingChatId = await _db.QueryAsync(context =>
            context.Chats
                .AsNoTracking()
                .Where(chat => chat.TrainerId == trainerId && chat.ClientId == clientId)
                .Select(chat => (Guid?)chat.Id)
                .SingleOrDefaultAsync());

        Chat? newChat = null;
        var chatId = existingChatId;
        if (!chatId.HasValue)
        {
            var chatResult = Chat.Create(trainerId, clientId, utcNow.AddDays(-2));
            chatResult.IsSuccess.Should().BeTrue();
            newChat = chatResult.Value;
            chatId = newChat.Id;
        }

        var mediaResult = MediaAsset.Create(
            clientId,
            MediaStorageProvider.LocalDev,
            $"reports/{Guid.NewGuid():N}.mp4",
            $"http://localhost/media/{Guid.NewGuid():N}.mp4",
            "report.mp4",
            "video/mp4",
            sizeBytes: 2048,
            MediaAssetKind.Video,
            durationSeconds: 30,
            utcNow.AddDays(-1));
        mediaResult.IsSuccess.Should().BeTrue();

        var reportResult = VideoReport.Create(
            chatId.Value,
            clientId,
            trainerId,
            "Technique review",
            "Please review",
            [mediaResult.Value.Id],
            utcNow.AddDays(-1));
        reportResult.IsSuccess.Should().BeTrue();

        if (reviewed)
        {
            reportResult.Value.Review("Looks good", utcNow).IsSuccess.Should().BeTrue();
        }

        await _db.ExecuteAsync(async context =>
        {
            if (newChat is not null)
            {
                await context.Chats.AddAsync(newChat);
            }

            await context.MediaAssets.AddAsync(mediaResult.Value);
            await context.VideoReports.AddAsync(reportResult.Value);
            await context.SaveChangesAsync();
        });
    }
}
