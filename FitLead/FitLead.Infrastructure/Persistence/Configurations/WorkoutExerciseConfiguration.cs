using FitLead.Domain.Trainings.Exercises;
using FitLead.Domain.Trainings.Workouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class WorkoutExerciseConfiguration
    : IEntityTypeConfiguration<WorkoutExercise>
    {
        public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
        {
            builder.ToTable("workout_exercises");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.ExerciseId)
                .IsRequired();

            builder.Property(x => x.WorkoutId)
                .HasColumnName("workout_id")
                .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.Repetitions)
                .IsRequired();

            builder.Property(x => x.Sets)
                .IsRequired();

            builder.Property(x => x.LoadKg);

            builder.Property(x => x.RestSeconds)
                .IsRequired();

            builder.Property(x => x.TrainerNote)
                .HasMaxLength(WorkoutExercise.MaxTrainerNoteLength);

            builder.HasIndex(x => x.ExerciseId);

            builder.HasOne<Exercise>()
                .WithMany()
                .HasForeignKey(x => x.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.WorkoutId);
            builder.HasIndex(x => new { x.WorkoutId, x.Order });

            builder.HasOne<Workout>()
                .WithMany(x => x.Exercises)
                .HasForeignKey(x => x.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
