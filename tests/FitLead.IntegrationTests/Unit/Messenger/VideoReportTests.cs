using FitLead.Domain.Messenger.VideoReports;
using FluentAssertions;

namespace FitLead.IntegrationTests.Unit.Messenger;

public sealed class VideoReportTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateSubmittedReportWithOrderedMedia()
    {
        var chatId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var trainerId = Guid.NewGuid();
        var mediaAssetIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var createdAtUtc = DateTime.UtcNow;

        var result = VideoReport.Create(
            chatId,
            clientId,
            trainerId,
            "  Squat check  ",
            "  Please review  ",
            mediaAssetIds,
            createdAtUtc);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(VideoReportStatus.Submitted);
        result.Value.Title.Should().Be("Squat check");
        result.Value.Description.Should().Be("Please review");
        result.Value.ReviewedAtUtc.Should().BeNull();
        result.Value.TrainerFeedbackText.Should().BeNull();
        result.Value.Media.Select(media => media.MediaAssetId).Should().Equal(mediaAssetIds);
        result.Value.Media.Select(media => media.OrderInReport).Should().Equal(1, 2);
    }

    [Fact]
    public void Create_WithoutMedia_ShouldReturnValidationError()
    {
        var result = Create(mediaAssetIds: []);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("video_report.create.media_required");
    }

    [Fact]
    public void Create_WithTooManyMedia_ShouldReturnValidationError()
    {
        var mediaAssetIds = Enumerable.Range(0, VideoReport.MaxMediaCount + 1)
            .Select(_ => Guid.NewGuid())
            .ToArray();

        var result = Create(mediaAssetIds: mediaAssetIds);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("video_report.create.media_limit_exceeded");
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldReturnValidationError()
    {
        var result = Create(title: " ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("video_report.create.title_required");
    }

    [Fact]
    public void Review_WhenSubmitted_ShouldSetReviewedState()
    {
        var report = Create().Value;
        var reviewedAtUtc = DateTime.UtcNow;

        var result = report.Review("  Keep your knees controlled.  ", reviewedAtUtc);

        result.IsSuccess.Should().BeTrue();
        report.Status.Should().Be(VideoReportStatus.Reviewed);
        report.ReviewedAtUtc.Should().Be(reviewedAtUtc);
        report.TrainerFeedbackText.Should().Be("Keep your knees controlled.");
    }

    [Fact]
    public void Review_WithTooLongFeedback_ShouldReturnValidationError()
    {
        var report = Create().Value;
        var feedback = new string('a', VideoReport.MaxTrainerFeedbackTextLength + 1);

        var result = report.Review(feedback, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("video_report.review.feedback_text_too_long");
    }

    [Fact]
    public void Review_WhenAlreadyReviewed_ShouldReturnConflict()
    {
        var report = Create().Value;
        report.Review("First review", DateTime.UtcNow).IsSuccess.Should().BeTrue();

        var result = report.Review("Second review", DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("video_report.review.already_reviewed");
    }

    private static FitLead.Common.Results.Result<VideoReport> Create(
        string title = "Squat check",
        IReadOnlyList<Guid>? mediaAssetIds = null)
    {
        return VideoReport.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            title,
            null,
            mediaAssetIds ?? [Guid.NewGuid()],
            DateTime.UtcNow);
    }
}
