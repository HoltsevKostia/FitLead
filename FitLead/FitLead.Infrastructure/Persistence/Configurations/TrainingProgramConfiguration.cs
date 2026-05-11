using FitLead.Domain.Trainings.TrainingPrograms;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{

    public sealed class TrainingProgramConfiguration
    : IEntityTypeConfiguration<TrainingProgram>
    {
        public void Configure(EntityTypeBuilder<TrainingProgram> builder)
        {

            builder.ToTable("training_programs", table =>
            {
                table.HasCheckConstraint(
                    "CK_training_programs_weeks_count_range",
                    $"\"WeeksCount\" BETWEEN 1 AND {TrainingProgram.MaxWeeksCount}");

                table.HasCheckConstraint(
                    "CK_training_programs_days_per_week_range",
                    $"\"DaysPerWeek\" BETWEEN 1 AND {TrainingProgram.MaxDaysPerWeek}");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.TrainerId)
                .IsRequired();

            builder.Property(x => x.WeeksCount)
                .IsRequired();

            builder.Property(x => x.DaysPerWeek)
                .IsRequired();

            builder.HasIndex(x => x.TrainerId);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(x => x.Workouts)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
