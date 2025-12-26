using FitLead.Domain.Trainings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{

    public sealed class TrainingProgramConfiguration
    : IEntityTypeConfiguration<TrainingProgram>
    {
        public void Configure(EntityTypeBuilder<TrainingProgram> builder)
        {
            builder.HasMany(x => x.Workouts)
                .WithOne()
                .HasForeignKey("training_program_id")
                .OnDelete(DeleteBehavior.Cascade);
            builder.ToTable("training_programs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.TrainerId)
                .IsRequired();

            builder.HasIndex(x => x.TrainerId);
        }
    }
}
