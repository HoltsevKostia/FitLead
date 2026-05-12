using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Domain.Trainings.TrainingPrograms;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLead.Infrastructure.Persistence.Configurations
{
    public sealed class AssignedTrainingProgramConfiguration
        : IEntityTypeConfiguration<AssignedTrainingProgram>
    {
        public void Configure(EntityTypeBuilder<AssignedTrainingProgram> builder)
        {
            builder.ToTable("assigned_training_programs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.TrainerId)
                .IsRequired();

            builder.Property(x => x.ClientId)
                .IsRequired();

            builder.Property(x => x.TrainingProgramId)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.AccessSource)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.AssignedAtUtc)
                .IsRequired();

            builder.Property(x => x.RevokedAtUtc)
                .IsRequired(false);

            builder.Property(x => x.ExpiresAtUtc)
                .IsRequired(false);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<TrainingProgram>()
                .WithMany()
                .HasForeignKey(x => x.TrainingProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ClientId, x.Status })
                .HasDatabaseName("IX_assigned_training_programs_client_id_status");

            builder.HasIndex(x => new { x.TrainerId, x.ClientId })
                .HasDatabaseName("IX_assigned_training_programs_trainer_id_client_id");

            builder.HasIndex(x => x.TrainingProgramId)
                .HasDatabaseName("IX_assigned_training_programs_training_program_id");

            builder.HasIndex(x => new { x.ClientId, x.TrainingProgramId })
                .IsUnique()
                .HasFilter("\"Status\" = 1")
                .HasDatabaseName("UX_assigned_training_programs_active_client_program");
        }
    }
}
