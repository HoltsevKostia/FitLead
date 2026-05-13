using FitLead.Domain.Trainings.TrainingPrograms;
using FitLead.Domain.Trainings.Workouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class TrainingProgramWorkoutConfiguration
    : IEntityTypeConfiguration<TrainingProgramWorkout>
    {
        public void Configure(EntityTypeBuilder<TrainingProgramWorkout> builder)
        {
            builder.ToTable("training_program_workouts", table =>
            {
                table.HasCheckConstraint(
                    "CK_training_program_workouts_week_number_positive",
                    "\"WeekNumber\" > 0");

                table.HasCheckConstraint(
                    "CK_training_program_workouts_day_number_positive",
                    "\"DayNumber\" > 0");

                table.HasCheckConstraint(
                    "CK_training_program_workouts_order_in_day_positive",
                    "\"OrderInDay\" > 0");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.TrainingProgramId)
                .IsRequired();

            builder.Property(x => x.WorkoutId)
                .IsRequired();

            builder.Property(x => x.WeekNumber)
                .IsRequired();

            builder.Property(x => x.DayNumber)
                .IsRequired();

            builder.Property(x => x.OrderInDay)
                .IsRequired();

            builder.HasIndex(x => x.WorkoutId);

            builder.HasIndex(x => new
            {
                x.TrainingProgramId,
                x.WeekNumber,
                x.DayNumber,
                x.OrderInDay
            });

            builder.HasOne<TrainingProgram>()
                .WithMany(x => x.Workouts)
                .HasForeignKey(x => x.TrainingProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Workout>()
                .WithMany()
                .HasForeignKey(x => x.WorkoutId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
