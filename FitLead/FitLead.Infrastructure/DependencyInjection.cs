using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Infrastructure.Persistence;
using FitLead.Infrastructure.Persistence.Repositories;
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
                    configuration.GetConnectionString("DefaultConnection")));

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

            return services;
        }
    }
}
