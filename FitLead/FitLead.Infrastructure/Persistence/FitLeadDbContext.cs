using FitLead.Domain.Trainings;
using FitLead.Domain.Users;
using FitLead.Infrastructure.Persistence.Models;
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
        public DbSet<User> Users => Set<User>();
        public DbSet<TrainerClient> TrainerClients => Set<TrainerClient>();
        public DbSet<Exercise> Exercises => Set<Exercise>();
        public DbSet<Workout> Workouts => Set<Workout>();
        public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitLeadDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
