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
        }
    }
}
