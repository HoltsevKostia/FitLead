using FitLead.Domain.Invitations;
using FitLead.Domain.Trainings;
using FitLead.Domain.Users;
using FitLead.Infrastructure.Identity;
using FitLead.Infrastructure.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence
{
    public class FitLeadDbContext : IdentityDbContext<AppIdentityUser, IdentityRole, string>
    {
        public FitLeadDbContext(DbContextOptions<FitLeadDbContext> options)
            : base(options)
        {
        }

        public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();
        public DbSet<User> DomainUsers => Set<User>();
        public DbSet<TrainerClient> TrainerClients => Set<TrainerClient>();
        public DbSet<Exercise> Exercises => Set<Exercise>();
        public DbSet<Workout> Workouts => Set<Workout>();
        public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
        public DbSet<Invitation> Invitations => Set<Invitation>();
        public DbSet<TrainingProgramWorkout> TrainingProgramWorkouts => Set<TrainingProgramWorkout>();
        public DbSet<UserIdentityLink> UserIdentityLinks => Set<UserIdentityLink>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitLeadDbContext).Assembly);
        }
    }
}
