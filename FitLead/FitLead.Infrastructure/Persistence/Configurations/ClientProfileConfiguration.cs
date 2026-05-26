using FitLead.Domain.Clients.ClientProfiles;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class ClientProfileConfiguration : IEntityTypeConfiguration<ClientProfile>
    {
        public void Configure(EntityTypeBuilder<ClientProfile> builder)
        {
            builder.ToTable("client_profiles", table =>
            {
                table.HasCheckConstraint(
                    "CK_client_profiles_experience_level_valid",
                    $"\"ExperienceLevel\" IS NULL OR \"ExperienceLevel\" IN ({(int)ClientExperienceLevel.Beginner}, {(int)ClientExperienceLevel.Intermediate}, {(int)ClientExperienceLevel.Advanced})");

                table.HasCheckConstraint(
                    "CK_client_profiles_height_range",
                    $"\"HeightCm\" IS NULL OR (\"HeightCm\" BETWEEN {ClientProfile.MinHeightCm} AND {ClientProfile.MaxHeightCm})");

                table.HasCheckConstraint(
                    "CK_client_profiles_updated_at_after_created",
                    "\"UpdatedAtUtc\" IS NULL OR \"UpdatedAtUtc\" >= \"CreatedAtUtc\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.ClientId)
                .IsRequired();

            builder.Property(x => x.Goal)
                .HasMaxLength(ClientProfile.MaxGoalLength)
                .IsRequired(false);

            builder.Property(x => x.ExperienceLevel)
                .HasConversion<int?>()
                .IsRequired(false);

            builder.Property(x => x.HeightCm)
                .IsRequired(false);

            builder.Property(x => x.Limitations)
                .HasMaxLength(ClientProfile.MaxLongTextLength)
                .IsRequired(false);

            builder.Property(x => x.TrainingPreferences)
                .HasMaxLength(ClientProfile.MaxLongTextLength)
                .IsRequired(false);

            builder.Property(x => x.AdditionalInfo)
                .HasMaxLength(ClientProfile.MaxLongTextLength)
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired(false);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ClientId)
                .IsUnique()
                .HasDatabaseName("UX_client_profiles_client_id");
        }
    }
}
