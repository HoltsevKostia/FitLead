using System.Net;
using FitLead.Application.TrainerVideoReports.Queries;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Messenger.VideoReports;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainerVideoReports;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class PendingVideoReportsTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private readonly TestDb _db = new(fixture);
    private readonly TestUsers _users = new(fixture, new TestDb(fixture));
    private readonly TestApiClients _api = new(fixture);

    [Fact]
    public async Task GetPending_ShouldReturnOnlyCurrentTrainerSubmittedReportsOldestFirst()
    {
        var trainer = await _users.RegisterTrainerAsync("pending-reports-trainer");
        var otherTrainer = await _users.RegisterTrainerAsync("pending-reports-other-trainer");
        var olderClient = await _users.RegisterClientAsync("pending-reports-older-client");
        var newerClient = await _users.RegisterClientAsync("pending-reports-newer-client");
        var reviewedClient = await _users.RegisterClientAsync("pending-reports-reviewed-client");
        var otherClient = await _users.RegisterClientAsync("pending-reports-other-client");
        var utcNow = DateTime.UtcNow;

        var olderReport = await CreateReportAsync(
            trainer.Id,
            olderClient.Id,
            "Older report",
            utcNow.AddDays(-2),
            mediaCount: 2);
        var newerReport = await CreateReportAsync(
            trainer.Id,
            newerClient.Id,
            "Newer report",
            utcNow.AddDays(-1));
        await CreateReportAsync(
            trainer.Id,
            reviewedClient.Id,
            "Reviewed report",
            utcNow.AddHours(-12),
            reviewed: true);
        await CreateReportAsync(
            otherTrainer.Id,
            otherClient.Id,
            "Other trainer report",
            utcNow.AddDays(-3));

        var reportsClient = await _api.TrainerVideoReportsAsync(trainer.Auth);
        var response = await reportsClient.GetPendingAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reports = await response.ReadRequiredJsonAsync<
            IReadOnlyList<TrainerPendingVideoReportDto>>();
        reports.Select(report => report.ReportId)
            .Should()
            .Equal(olderReport.Id, newerReport.Id);
        reports[0].ChatId.Should().Be(olderReport.ChatId);
        reports[0].ClientId.Should().Be(olderClient.Id);
        reports[0].ClientName.Should().Be("Test Client");
        reports[0].Title.Should().Be("Older report");
        reports[0].Description.Should().Be("Please review");
        reports[0].MediaCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPending_AsClient_ShouldReturnForbidden()
    {
        var client = await _users.RegisterClientAsync("pending-reports-forbidden-client");
        var reportsClient = await _api.TrainerVideoReportsAsync(client.Auth);

        var response = await reportsClient.GetPendingAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPending_Unauthenticated_ShouldReturnUnauthorized()
    {
        using var anonymous = Fixture.CreateClient(handleCookies: false);

        var response = await anonymous.GetAsync("/api/trainer/video-reports/pending");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<VideoReport> CreateReportAsync(
        Guid trainerId,
        Guid clientId,
        string title,
        DateTime createdAtUtc,
        int mediaCount = 1,
        bool reviewed = false)
    {
        var chatResult = Chat.Create(trainerId, clientId, createdAtUtc.AddMinutes(-1));
        chatResult.IsSuccess.Should().BeTrue();

        var mediaAssets = Enumerable.Range(0, mediaCount)
            .Select(index => MediaAsset.Create(
                clientId,
                MediaStorageProvider.Uploadcare,
                $"pending-reports/{Guid.NewGuid():N}",
                $"https://ucarecdn.example/{Guid.NewGuid():N}/",
                $"report-{index}.mp4",
                "video/mp4",
                sizeBytes: 1024,
                MediaAssetKind.Video,
                durationSeconds: 30,
                createdAtUtc.AddMinutes(-1)))
            .ToList();
        mediaAssets.Should().OnlyContain(result => result.IsSuccess);

        var reportResult = VideoReport.Create(
            chatResult.Value.Id,
            clientId,
            trainerId,
            title,
            "Please review",
            mediaAssets.Select(result => result.Value.Id).ToList(),
            createdAtUtc);
        reportResult.IsSuccess.Should().BeTrue();

        if (reviewed)
        {
            reportResult.Value.Review(
                "Reviewed",
                createdAtUtc.AddMinutes(1)).IsSuccess.Should().BeTrue();
        }

        await _db.ExecuteAsync(async context =>
        {
            await context.Chats.AddAsync(chatResult.Value);
            await context.MediaAssets.AddRangeAsync(
                mediaAssets.Select(result => result.Value));
            await context.VideoReports.AddAsync(reportResult.Value);
            await context.SaveChangesAsync();
        });

        return reportResult.Value;
    }
}
