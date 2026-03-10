using EntityFramework.Exceptions.PostgreSQL;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Deletion;
using FitLead.Application.Common.Results;
using FitLead.Application.Common.Time;
using FitLead.Application.Identity;
using FitLead.Application.Invitations.Access;
using FitLead.Application.Trainings.Exercises.Access;
using FitLead.Application.Trainings.TrainingPrograms.Access;
using FitLead.Application.Trainings.Workouts.Access;
using FitLead.Infrastructure.Deletion;
using FitLead.Infrastructure.Identity;
using FitLead.Infrastructure.Persistence;
using FitLead.Infrastructure.Persistence.Repositories;
using FitLead.Infrastructure.Time;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.Infrastructure
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<FitLeadDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"))
                    .UseExceptionProcessor());

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserReadRepository, UserReadRepository>();
            services.AddScoped<ITrainingProgramRepository, TrainingProgramRepository>();
            services.AddScoped<ITrainingProgramReadRepository, TrainingProgramReadRepository>();
            services.AddScoped<ITrainerClientRepository, TrainerClientRepository>();
            services.AddScoped<ITrainerClientReadRepository, TrainerClientReadRepository>();
            services.AddScoped<IExerciseRepository, ExerciseRepository>();
            services.AddScoped<IExerciseReadRepository, ExerciseReadRepository>();
            services.AddScoped<IWorkoutRepository, WorkoutRepository>();
            services.AddScoped<IWorkoutReadRepository, WorkoutReadRepository>();
            services.AddScoped<IInvitationRepository, InvitationRepository>();
            services.AddScoped<IInvitationReadRepository, InvitationReadRepository>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DomainExceptionToResultBehavior<,>));
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<IClock, SystemClock>();
            services.AddDataProtection();
            services.Configure<DeletionTokenOptions>(
                configuration.GetSection(DeletionTokenOptions.SectionName));
            services.AddSingleton<IDeletionConfirmationTokenService, DataProtectionDeletionConfirmationTokenService>();
            services.AddScoped<ITokenHasher, TokenHasher>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IIdentityDomainUserLinkResolver, IdentityDomainUserLinkResolver>();
            services.AddScoped<IWorkoutLoader, WorkoutLoader>();
            services.AddScoped<IExerciseLoader, ExerciseLoader>();
            services.AddScoped<ITrainingProgramLoader, TrainingProgramLoader>();
            services.AddScoped<IInvitationLoader, InvitationLoader>();

            return services;
        }
    }
}
