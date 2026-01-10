using FitLead.Domain.Trainings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class TrainingProgramWorkoutConfiguration
    : IEntityTypeConfiguration<TrainingProgramWorkout>
    {
        public void Configure(EntityTypeBuilder<TrainingProgramWorkout> builder)
        {
            builder.ToTable("training_program_workouts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.TrainingProgramId)
                .IsRequired();

            builder.Property(x => x.WorkoutId)
                .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired();

            builder.HasIndex(x => x.TrainingProgramId);

            builder.HasOne<TrainingProgram>()
                .WithMany(x => x.Workouts)
                .HasForeignKey(x => x.TrainingProgramId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
