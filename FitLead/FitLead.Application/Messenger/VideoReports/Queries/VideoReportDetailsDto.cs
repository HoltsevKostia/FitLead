namespace FitLead.Application.Messenger.VideoReports.Queries
{
    public sealed record VideoReportDetailsDto(
        Guid Id,
        Guid ChatId,
        Guid ClientId,
        Guid TrainerId,
        string Title,
        string? Description,
        string Status,
        DateTime CreatedAtUtc,
        DateTime? ReviewedAtUtc,
        string? TrainerFeedbackText,
        IReadOnlyList<VideoReportMediaDto> Media);
}
