using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Messenger.VideoReports;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class VideoReportConfiguration : IEntityTypeConfiguration<VideoReport>
    {
        public void Configure(EntityTypeBuilder<VideoReport> builder)
        {
            builder.ToTable("video_reports");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.ChatId)
                .IsRequired();

            builder.Property(x => x.ClientId)
                .IsRequired();

            builder.Property(x => x.TrainerId)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(VideoReport.MaxTitleLength)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(VideoReport.MaxDescriptionLength)
                .IsRequired(false);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.ReviewedAtUtc)
                .IsRequired(false);

            builder.Property(x => x.TrainerFeedbackText)
                .HasMaxLength(VideoReport.MaxTrainerFeedbackTextLength)
                .IsRequired(false);

            builder.HasOne<Chat>()
                .WithMany()
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Media)
                .WithOne()
                .HasForeignKey(x => x.VideoReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Media)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => x.ChatId)
                .HasDatabaseName("IX_video_reports_chat_id");

            builder.HasIndex(x => x.ClientId)
                .HasDatabaseName("IX_video_reports_client_id");

            builder.HasIndex(x => x.TrainerId)
                .HasDatabaseName("IX_video_reports_trainer_id");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_video_reports_status");
        }
    }
}
