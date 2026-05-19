using FitLead.Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages", table =>
            {
                table.HasCheckConstraint(
                    "CK_outbox_messages_retry_count_non_negative",
                    "\"RetryCount\" >= 0");

                table.HasCheckConstraint(
                    "CK_outbox_messages_status_valid",
                    "\"Status\" IN (1, 2, 3)");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Type)
                .HasMaxLength(OutboxMessage.MaxTypeLength)
                .IsRequired();

            builder.Property(x => x.Payload)
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(x => x.OccurredAtUtc)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.RetryCount)
                .IsRequired();

            builder.Property(x => x.NextRetryAtUtc)
                .IsRequired(false);

            builder.Property(x => x.ProcessedAtUtc)
                .IsRequired(false);

            builder.Property(x => x.Error)
                .HasMaxLength(OutboxMessage.MaxErrorLength)
                .IsRequired(false);

            builder.HasIndex(x => new { x.Status, x.NextRetryAtUtc, x.OccurredAtUtc })
                .HasDatabaseName("IX_outbox_messages_status_next_retry_occurred");

            builder.HasIndex(x => x.Type)
                .HasDatabaseName("IX_outbox_messages_type");
        }
    }
}
