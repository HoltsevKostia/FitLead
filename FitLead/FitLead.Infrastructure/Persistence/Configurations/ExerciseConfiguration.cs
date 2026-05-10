using FitLead.Domain.Trainings;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class ExerciseConfiguration
    : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.ToTable("exercises", table =>
            {
                table.HasCheckConstraint(
                    "CK_exercises_platform_owner_null",
                    "\"Source\" <> 1 OR \"OwnerTrainerId\" IS NULL");

                table.HasCheckConstraint(
                    "CK_exercises_trainer_owner_required",
                    "\"Source\" <> 2 OR \"OwnerTrainerId\" IS NOT NULL");

                table.HasCheckConstraint(
                    "CK_exercises_copied_from_trainer_only",
                    "\"CopiedFromExerciseId\" IS NULL OR \"Source\" = 2");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.OwnerTrainerId);

            builder.Property(x => x.Source)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.CopiedFromExerciseId);

            builder.Property(x => x.MuscleGroup)
                .HasConversion<int?>();

            builder.Property(x => x.Equipment)
                .HasConversion<int?>();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.OwnerTrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Exercise>()
                .WithMany()
                .HasForeignKey(x => x.CopiedFromExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired();

            builder.Property(x => x.MediaUrl)
                .HasConversion(
                    mediaUrl => mediaUrl == null ? null : mediaUrl.Value,
                    value => value == null ? null : MediaUrl.Create(value).Value)
                .HasMaxLength(MediaUrl.MaxLength);

            builder.HasIndex(x => x.OwnerTrainerId);
            builder.HasIndex(x => x.Source);
            builder.HasIndex(x => x.CopiedFromExerciseId);
            builder.HasIndex(x => x.MuscleGroup);
            builder.HasIndex(x => x.Equipment);

            builder.HasIndex(x => new { x.OwnerTrainerId, x.CopiedFromExerciseId })
                .IsUnique()
                .HasFilter("\"CopiedFromExerciseId\" IS NOT NULL");
        }
    }
}
