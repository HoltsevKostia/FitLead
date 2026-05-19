using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Messenger.VideoReports
{
    public sealed class VideoReport : AggregateRoot<Guid>
    {
        public const int MaxTitleLength = 200;
        public const int MaxDescriptionLength = 2000;
        public const int MaxTrainerFeedbackTextLength = 4000;
        public const int MaxMediaCount = 5;

        private readonly List<VideoReportMedia> _media = [];

        public Guid ChatId { get; private set; }
        public Guid ClientId { get; private set; }
        public Guid TrainerId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public VideoReportStatus Status { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? ReviewedAtUtc { get; private set; }
        public string? TrainerFeedbackText { get; private set; }
        public IReadOnlyCollection<VideoReportMedia> Media => _media.AsReadOnly();

        private VideoReport()
        {
        }

        private VideoReport(
            Guid id,
            Guid chatId,
            Guid clientId,
            Guid trainerId,
            string title,
            string? description,
            DateTime createdAtUtc,
            IReadOnlyList<Guid> mediaAssetIds)
        {
            Id = id;
            ChatId = chatId;
            ClientId = clientId;
            TrainerId = trainerId;
            Title = title;
            Description = description;
            Status = VideoReportStatus.Submitted;
            CreatedAtUtc = createdAtUtc;

            for (var index = 0; index < mediaAssetIds.Count; index++)
            {
                _media.Add(
                    new VideoReportMedia(
                        Guid.NewGuid(),
                        id,
                        mediaAssetIds[index],
                        index + 1));
            }
        }

        public static Result<VideoReport> Create(
            Guid chatId,
            Guid clientId,
            Guid trainerId,
            string title,
            string? description,
            IReadOnlyList<Guid> mediaAssetIds,
            DateTime createdAtUtc)
        {
            if (chatId == Guid.Empty)
            {
                return Result<VideoReport>.Failure(
                    Error.Validation("video_report.create.chat_id_required", "ChatId is required"));
            }

            if (clientId == Guid.Empty)
            {
                return Result<VideoReport>.Failure(
                    Error.Validation("video_report.create.client_id_required", "ClientId is required"));
            }

            if (trainerId == Guid.Empty)
            {
                return Result<VideoReport>.Failure(
                    Error.Validation("video_report.create.trainer_id_required", "TrainerId is required"));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return Result<VideoReport>.Failure(
                    Error.Validation("video_report.create.title_required", "Title is required"));
            }

            var trimmedTitle = title.Trim();
            if (trimmedTitle.Length > MaxTitleLength)
            {
                return Result<VideoReport>.Failure(
                    Error.Validation("video_report.create.title_too_long", $"Title cannot exceed {MaxTitleLength} characters"));
            }

            var descriptionValidation = ValidateDescription(description);
            if (descriptionValidation.IsFailure)
            {
                return Result<VideoReport>.Failure(descriptionValidation.Error);
            }

            if (mediaAssetIds is null)
            {
                return Result<VideoReport>.Failure(
                    Error.Validation("video_report.create.media_required", "Media is required"));
            }

            if (mediaAssetIds.Count == 0)
            {
                return Result<VideoReport>.Failure(
                    Error.Validation("video_report.create.media_required", "At least one media asset is required"));
            }

            if (mediaAssetIds.Count > MaxMediaCount)
            {
                return Result<VideoReport>.Failure(
                    Error.Validation("video_report.create.media_limit_exceeded", $"Video report cannot contain more than {MaxMediaCount} media assets"));
            }

            if (mediaAssetIds.Any(mediaAssetId => mediaAssetId == Guid.Empty))
            {
                return Result<VideoReport>.Failure(
                    Error.Validation("video_report.create.media_asset_id_required", "MediaAssetId is required"));
            }

            if (createdAtUtc == default)
            {
                return Result<VideoReport>.Failure(
                    Error.Validation("video_report.create.created_at_required", "CreatedAtUtc is required"));
            }

            return Result<VideoReport>.Success(
                new VideoReport(
                    Guid.NewGuid(),
                    chatId,
                    clientId,
                    trainerId,
                    trimmedTitle,
                    NormalizeDescription(description),
                    createdAtUtc,
                    mediaAssetIds));
        }

        public Result Review(
            string feedbackText,
            DateTime reviewedAtUtc)
        {
            if (Status != VideoReportStatus.Submitted)
            {
                return Result.Failure(
                    Error.Conflict("video_report.review.already_reviewed", "Video report is already reviewed"));
            }

            if (string.IsNullOrWhiteSpace(feedbackText))
            {
                return Result.Failure(
                    Error.Validation("video_report.review.feedback_text_required", "Feedback text is required"));
            }

            var trimmedFeedbackText = feedbackText.Trim();
            if (trimmedFeedbackText.Length > MaxTrainerFeedbackTextLength)
            {
                return Result.Failure(
                    Error.Validation(
                        "video_report.review.feedback_text_too_long",
                        $"Feedback text cannot exceed {MaxTrainerFeedbackTextLength} characters"));
            }

            if (reviewedAtUtc == default)
            {
                return Result.Failure(
                    Error.Validation("video_report.review.reviewed_at_required", "ReviewedAtUtc is required"));
            }

            Status = VideoReportStatus.Reviewed;
            ReviewedAtUtc = reviewedAtUtc;
            TrainerFeedbackText = trimmedFeedbackText;

            return Result.Success();
        }

        private static Result ValidateDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return Result.Success();
            }

            var trimmedDescription = description.Trim();
            if (trimmedDescription.Length > MaxDescriptionLength)
            {
                return Result.Failure(
                    Error.Validation("video_report.create.description_too_long", $"Description cannot exceed {MaxDescriptionLength} characters"));
            }

            return Result.Success();
        }

        private static string? NormalizeDescription(string? description)
        {
            return string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim();
        }
    }
}
