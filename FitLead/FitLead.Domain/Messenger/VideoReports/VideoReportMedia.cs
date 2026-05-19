using FitLead.Common.Domain;

namespace FitLead.Domain.Messenger.VideoReports
{
    public sealed class VideoReportMedia : Entity<Guid>
    {
        public Guid VideoReportId { get; private set; }
        public Guid MediaAssetId { get; private set; }
        public int OrderInReport { get; private set; }

        private VideoReportMedia()
        {
        }

        internal VideoReportMedia(
            Guid id,
            Guid videoReportId,
            Guid mediaAssetId,
            int orderInReport)
        {
            Id = id;
            VideoReportId = videoReportId;
            MediaAssetId = mediaAssetId;
            OrderInReport = orderInReport;
        }
    }
}
