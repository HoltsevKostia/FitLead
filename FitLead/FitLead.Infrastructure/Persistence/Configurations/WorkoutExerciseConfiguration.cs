using FitLead.Domain.Trainings;
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

            builder.Property(x => x.Repetitions)
                .IsRequired();

            builder.Property(x => x.Sets)
                .IsRequired();

            builder.Property(x => x.RestSeconds)
                .IsRequired();

            builder.HasIndex(x => x.ExerciseId);

            builder.HasOne<Exercise>()
                .WithMany()
                .HasForeignKey(x => x.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property<Guid>("workout_id")
                .IsRequired();

            builder.HasIndex("workout_id");

            builder.HasOne<Workout>()
                .WithMany(x => x.Exercises)
                .HasForeignKey("workout_id")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
