using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
    {
        public void Configure(EntityTypeBuilder<MediaAsset> builder)
        {
            builder.ToTable("media_assets");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.OwnerUserId)
                .IsRequired();

            builder.Property(x => x.StorageProvider)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.StorageObjectId)
                .HasMaxLength(MediaAsset.MaxStorageObjectIdLength)
                .IsRequired();

            builder.Property(x => x.DeliveryUrl)
                .HasMaxLength(MediaAsset.MaxDeliveryUrlLength)
                .IsRequired();

            builder.Property(x => x.FileName)
                .HasMaxLength(MediaAsset.MaxFileNameLength)
                .IsRequired(false);

            builder.Property(x => x.ContentType)
                .HasMaxLength(MediaAsset.MaxContentTypeLength)
                .IsRequired();

            builder.Property(x => x.SizeBytes)
                .IsRequired();

            builder.Property(x => x.Kind)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.DurationSeconds)
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.OwnerUserId)
                .HasDatabaseName("IX_media_assets_owner_user_id");

            builder.HasIndex(x => new { x.StorageProvider, x.StorageObjectId })
                .IsUnique()
                .HasDatabaseName("UX_media_assets_storage_provider_object_id");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_media_assets_status");

            builder.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_media_assets_size_bytes_positive",
                    "\"SizeBytes\" > 0");

                table.HasCheckConstraint(
                    "CK_media_assets_duration_seconds_positive",
                    "\"DurationSeconds\" IS NULL OR \"DurationSeconds\" > 0");
            });
        }
    }
}
