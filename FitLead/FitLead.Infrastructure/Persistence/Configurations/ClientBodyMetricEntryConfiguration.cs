using FitLead.Domain.Clients.BodyMetrics;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class ClientBodyMetricEntryConfiguration : IEntityTypeConfiguration<ClientBodyMetricEntry>
    {
        public void Configure(EntityTypeBuilder<ClientBodyMetricEntry> builder)
        {
            builder.ToTable("client_body_metric_entries", table =>
            {
                table.HasCheckConstraint(
                    "CK_client_body_metric_entries_weight_range",
                    $"\"WeightKg\" IS NULL OR (\"WeightKg\" BETWEEN {ClientBodyMetricEntry.MinWeightKg} AND {ClientBodyMetricEntry.MaxWeightKg})");

                table.HasCheckConstraint(
                    "CK_client_body_metric_entries_body_fat_range",
                    $"\"BodyFatPercent\" IS NULL OR (\"BodyFatPercent\" BETWEEN {ClientBodyMetricEntry.MinBodyFatPercent} AND {ClientBodyMetricEntry.MaxBodyFatPercent})");

                table.HasCheckConstraint(
                    "CK_client_body_metric_entries_measurements_range",
                    $"(\"ChestCm\" IS NULL OR (\"ChestCm\" BETWEEN {ClientBodyMetricEntry.MinMeasurementCm} AND {ClientBodyMetricEntry.MaxMeasurementCm})) AND " +
                    $"(\"WaistCm\" IS NULL OR (\"WaistCm\" BETWEEN {ClientBodyMetricEntry.MinMeasurementCm} AND {ClientBodyMetricEntry.MaxMeasurementCm})) AND " +
                    $"(\"HipsCm\" IS NULL OR (\"HipsCm\" BETWEEN {ClientBodyMetricEntry.MinMeasurementCm} AND {ClientBodyMetricEntry.MaxMeasurementCm})) AND " +
                    $"(\"ArmCm\" IS NULL OR (\"ArmCm\" BETWEEN {ClientBodyMetricEntry.MinMeasurementCm} AND {ClientBodyMetricEntry.MaxMeasurementCm})) AND " +
                    $"(\"ThighCm\" IS NULL OR (\"ThighCm\" BETWEEN {ClientBodyMetricEntry.MinMeasurementCm} AND {ClientBodyMetricEntry.MaxMeasurementCm}))");

                table.HasCheckConstraint(
                    "CK_client_body_metric_entries_not_empty",
                    "\"WeightKg\" IS NOT NULL OR \"BodyFatPercent\" IS NOT NULL OR \"ChestCm\" IS NOT NULL OR \"WaistCm\" IS NOT NULL OR \"HipsCm\" IS NOT NULL OR \"ArmCm\" IS NOT NULL OR \"ThighCm\" IS NOT NULL OR \"Note\" IS NOT NULL");

                table.HasCheckConstraint(
                    "CK_client_body_metric_entries_updated_at_after_created",
                    "\"UpdatedAtUtc\" IS NULL OR \"UpdatedAtUtc\" >= \"CreatedAtUtc\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.ClientId)
                .IsRequired();

            builder.Property(x => x.RecordedAt)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.WeightKg)
                .HasPrecision(6, 2)
                .IsRequired(false);

            builder.Property(x => x.BodyFatPercent)
                .HasPrecision(5, 2)
                .IsRequired(false);

            builder.Property(x => x.ChestCm)
                .HasPrecision(6, 2)
                .IsRequired(false);

            builder.Property(x => x.WaistCm)
                .HasPrecision(6, 2)
                .IsRequired(false);

            builder.Property(x => x.HipsCm)
                .HasPrecision(6, 2)
                .IsRequired(false);

            builder.Property(x => x.ArmCm)
                .HasPrecision(6, 2)
                .IsRequired(false);

            builder.Property(x => x.ThighCm)
                .HasPrecision(6, 2)
                .IsRequired(false);

            builder.Property(x => x.Note)
                .HasMaxLength(ClientBodyMetricEntry.MaxNoteLength)
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired(false);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.ClientId, x.RecordedAt })
                .IsUnique()
                .HasDatabaseName("UX_client_body_metric_entries_client_id_recorded_at");
        }
    }
}
