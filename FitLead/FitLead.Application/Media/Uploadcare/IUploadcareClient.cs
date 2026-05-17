namespace FitLead.Application.Media.Uploadcare
{
    public interface IUploadcareClient
    {
        Task<UploadcareFileInfo?> GetFileInfoAsync(
            string uuid,
            CancellationToken cancellationToken);
    }
}
