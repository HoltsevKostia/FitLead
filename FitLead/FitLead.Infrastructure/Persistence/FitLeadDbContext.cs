using FitLead.Domain.Invitations;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Domain.Messenger.ChatMessages;
using FitLead.Domain.Messenger.Chats;
using FitLead.Domain.Messenger.VideoReports;
using FitLead.Domain.Notifications;
using FitLead.Domain.Notifications.PushSubscriptions;
using FitLead.Domain.Outbox;
using FitLead.Domain.Trainings.Exercises;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Domain.Trainings.TrainingPrograms;
using FitLead.Domain.Trainings.Workouts;
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
        public DbSet<Chat> Chats => Set<Chat>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<VideoReport> VideoReports => Set<VideoReport>();
        public DbSet<VideoReportMedia> VideoReportMedia => Set<VideoReportMedia>();
        public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<AssignedTrainingProgram> AssignedTrainingPrograms => Set<AssignedTrainingProgram>();
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
