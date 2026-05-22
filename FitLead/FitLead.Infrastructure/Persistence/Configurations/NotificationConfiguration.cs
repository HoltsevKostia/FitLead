using FitLead.Domain.Notifications;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("notifications", table =>
            {
                table.HasCheckConstraint(
                    "CK_notifications_type_valid",
                    $"\"Type\" IN ({(int)NotificationType.VideoReportSubmitted}, {(int)NotificationType.VideoReportReviewed}, {(int)NotificationType.TrainingProgramAssigned})");

                table.HasCheckConstraint(
                    "CK_notifications_read_state_valid",
                    "(\"IsRead\" = false AND \"ReadAtUtc\" IS NULL) OR (\"IsRead\" = true AND \"ReadAtUtc\" IS NOT NULL)");

                table.HasCheckConstraint(
                    "CK_notifications_read_at_after_created",
                    "\"ReadAtUtc\" IS NULL OR \"ReadAtUtc\" >= \"CreatedAtUtc\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.RecipientUserId)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Title)
                .HasMaxLength(Notification.MaxTitleLength)
                .IsRequired();

            builder.Property(x => x.Body)
                .HasMaxLength(Notification.MaxBodyLength)
                .IsRequired(false);

            builder.Property(x => x.LinkUrl)
                .HasMaxLength(Notification.MaxLinkUrlLength)
                .IsRequired();

            builder.Property(x => x.IsRead)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.ReadAtUtc)
                .IsRequired(false);

            builder.Property(x => x.SourceEventId)
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.RecipientUserId, x.IsRead, x.CreatedAtUtc })
                .HasDatabaseName("IX_notifications_recipient_read_created");

            builder.HasIndex(x => new { x.RecipientUserId, x.CreatedAtUtc })
                .HasDatabaseName("IX_notifications_recipient_created");

            builder.HasIndex(x => new { x.SourceEventId, x.RecipientUserId, x.Type })
                .IsUnique()
                .HasDatabaseName("UX_notifications_source_event_recipient_type");
        }
    }
}
