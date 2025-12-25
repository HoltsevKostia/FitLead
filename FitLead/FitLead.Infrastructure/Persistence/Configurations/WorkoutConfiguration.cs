using FitLead.Domain.Trainings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class WorkoutConfiguration
    : IEntityTypeConfiguration<Workout>
    {
        public void Configure(EntityTypeBuilder<Workout> builder)
        {
            builder.ToTable("workouts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.TrainerId)
                .IsRequired();

            builder.HasMany(x => x.Exercises)
                .WithOne()
                .HasForeignKey("workout_id")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
