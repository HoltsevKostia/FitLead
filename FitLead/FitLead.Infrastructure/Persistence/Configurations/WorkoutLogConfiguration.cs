using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Domain.Trainings.TrainingPrograms;
using FitLead.Domain.Trainings.WorkoutLogs;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class WorkoutLogConfiguration : IEntityTypeConfiguration<WorkoutLog>
    {
        public void Configure(EntityTypeBuilder<WorkoutLog> builder)
        {
            builder.ToTable("workout_logs", table =>
            {
                table.HasCheckConstraint(
                    "CK_workout_logs_status_valid",
                    $"\"Status\" IN ({(int)WorkoutLogStatus.Completed}, {(int)WorkoutLogStatus.Skipped})");

                table.HasCheckConstraint(
                    "CK_workout_logs_completed_performed_at_required",
                    $"\"Status\" <> {(int)WorkoutLogStatus.Completed} OR \"PerformedAtUtc\" IS NOT NULL");

                table.HasCheckConstraint(
                    "CK_workout_logs_skipped_fields_null",
                    $"\"Status\" <> {(int)WorkoutLogStatus.Skipped} OR (\"PerformedAtUtc\" IS NULL AND \"DifficultyRating\" IS NULL)");

                table.HasCheckConstraint(
                    "CK_workout_logs_difficulty_rating_range",
                    $"\"DifficultyRating\" IS NULL OR (\"DifficultyRating\" BETWEEN {WorkoutLog.MinDifficultyRating} AND {WorkoutLog.MaxDifficultyRating})");

                table.HasCheckConstraint(
                    "CK_workout_logs_updated_at_after_created",
                    "\"UpdatedAtUtc\" IS NULL OR \"UpdatedAtUtc\" >= \"CreatedAtUtc\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.AssignedTrainingProgramId)
                .IsRequired();

            builder.Property(x => x.TrainingProgramWorkoutId)
                .IsRequired();

            builder.Property(x => x.ClientId)
                .IsRequired();

            builder.Property(x => x.TrainerId)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.PerformedAtUtc)
                .IsRequired(false);

            builder.Property(x => x.ClientNote)
                .HasMaxLength(WorkoutLog.MaxClientNoteLength)
                .IsRequired(false);

            builder.Property(x => x.DifficultyRating)
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired(false);

            builder.HasOne<AssignedTrainingProgram>()
                .WithMany()
                .HasForeignKey(x => x.AssignedTrainingProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<TrainingProgramWorkout>()
                .WithMany()
                .HasForeignKey(x => x.TrainingProgramWorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AssignedTrainingProgramId, x.TrainingProgramWorkoutId })
                .IsUnique()
                .HasDatabaseName("UX_workout_logs_assignment_program_workout");

            builder.HasIndex(x => x.ClientId)
                .HasDatabaseName("IX_workout_logs_client_id");

            builder.HasIndex(x => x.TrainerId)
                .HasDatabaseName("IX_workout_logs_trainer_id");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_workout_logs_status");
        }
    }
}
