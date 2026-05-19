using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Messenger.VideoReports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class VideoReportMediaConfiguration : IEntityTypeConfiguration<VideoReportMedia>
    {
        public void Configure(EntityTypeBuilder<VideoReportMedia> builder)
        {
            builder.ToTable("video_report_media");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.VideoReportId)
                .IsRequired();

            builder.Property(x => x.MediaAssetId)
                .IsRequired();

            builder.Property(x => x.OrderInReport)
                .IsRequired();

            builder.HasOne<MediaAsset>()
                .WithMany()
                .HasForeignKey(x => x.MediaAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.VideoReportId, x.OrderInReport })
                .IsUnique()
                .HasDatabaseName("UX_video_report_media_report_id_order_in_report");

            builder.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_video_report_media_order_in_report_positive",
                    "\"OrderInReport\" > 0");
            });
        }
    }
}
