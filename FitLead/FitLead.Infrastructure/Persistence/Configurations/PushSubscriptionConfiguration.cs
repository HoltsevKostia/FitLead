using FitLead.Domain.Notifications.PushSubscriptions;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
    {
        public void Configure(EntityTypeBuilder<PushSubscription> builder)
        {
            builder.ToTable("push_subscriptions", table =>
            {
                table.HasCheckConstraint(
                    "CK_push_subscriptions_revoked_at_after_created",
                    "\"RevokedAtUtc\" IS NULL OR \"RevokedAtUtc\" >= \"CreatedAtUtc\"");

                table.HasCheckConstraint(
                    "CK_push_subscriptions_last_used_at_after_created",
                    "\"LastUsedAtUtc\" IS NULL OR \"LastUsedAtUtc\" >= \"CreatedAtUtc\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Endpoint)
                .HasMaxLength(PushSubscription.MaxEndpointLength)
                .IsRequired();

            builder.Property(x => x.P256dh)
                .HasMaxLength(PushSubscription.MaxKeyLength)
                .IsRequired();

            builder.Property(x => x.Auth)
                .HasMaxLength(PushSubscription.MaxKeyLength)
                .IsRequired();

            builder.Property(x => x.UserAgent)
                .HasMaxLength(PushSubscription.MaxUserAgentLength)
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.LastUsedAtUtc)
                .IsRequired(false);

            builder.Property(x => x.RevokedAtUtc)
                .IsRequired(false);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_push_subscriptions_user_id");

            builder.HasIndex(x => x.Endpoint)
                .IsUnique()
                .HasDatabaseName("UX_push_subscriptions_endpoint");

            builder.HasIndex(x => new { x.UserId, x.RevokedAtUtc })
                .HasDatabaseName("IX_push_subscriptions_user_revoked");
        }
    }
}
