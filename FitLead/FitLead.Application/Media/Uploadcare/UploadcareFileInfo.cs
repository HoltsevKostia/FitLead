namespace FitLead.Application.Media.Uploadcare
{
    public sealed record UploadcareFileInfo(
        string Uuid,
        string OriginalFileUrl,
        string? OriginalFilename,
        string MimeType,
        long Size,
        int? DurationMilliseconds);
}
