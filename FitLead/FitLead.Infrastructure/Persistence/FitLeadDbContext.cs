using FitLead.Domain.Trainings;
using FitLead.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence
{
    public class FitLeadDbContext : DbContext
    {
        public FitLeadDbContext(DbContextOptions<FitLeadDbContext> options)
            : base(options)
        {
        }

        public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();
        public DbSet<Trainer> Trainers => Set<Trainer>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitLeadDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
