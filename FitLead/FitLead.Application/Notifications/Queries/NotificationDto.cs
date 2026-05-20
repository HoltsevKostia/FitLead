namespace FitLead.Application.Notifications.Queries
{
    public sealed record NotificationDto(
        Guid Id,
        string Type,
        string Title,
        string? Body,
        string LinkUrl,
        bool IsRead,
        DateTime CreatedAtUtc,
        DateTime? ReadAtUtc);
}
