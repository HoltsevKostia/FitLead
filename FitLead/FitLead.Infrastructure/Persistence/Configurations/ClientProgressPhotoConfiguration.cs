using FitLead.Domain.Clients.ProgressPhotos;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class ClientProgressPhotoConfiguration : IEntityTypeConfiguration<ClientProgressPhoto>
    {
        public void Configure(EntityTypeBuilder<ClientProgressPhoto> builder)
        {
            builder.ToTable("client_progress_photos", table =>
            {
                table.HasCheckConstraint(
                    "CK_client_progress_photos_label_valid",
                    $"\"Label\" IN ({(int)ProgressPhotoLabel.Front}, {(int)ProgressPhotoLabel.Side}, {(int)ProgressPhotoLabel.Back}, {(int)ProgressPhotoLabel.Other})");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.ClientId)
                .IsRequired();

            builder.Property(x => x.MediaAssetId)
                .IsRequired();

            builder.Property(x => x.TakenAt)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.Label)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Note)
                .HasMaxLength(ClientProgressPhoto.MaxNoteLength)
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<MediaAsset>()
                .WithMany()
                .HasForeignKey(x => x.MediaAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ClientId, x.TakenAt })
                .HasDatabaseName("IX_client_progress_photos_client_id_taken_at");

            builder.HasIndex(x => x.MediaAssetId)
                .HasDatabaseName("IX_client_progress_photos_media_asset_id");
        }
    }
}