namespace FitLead.Application.Media.Uploadcare
{
    public sealed record UploadcareUploadSignature(
        string SecureSignature,
        string SecureExpire);
}
