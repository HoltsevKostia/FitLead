using EntityFramework.Exceptions.PostgreSQL;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Deletion;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Common.Results;
using FitLead.Application.Common.Time;
using FitLead.Application.Identity;
using FitLead.Application.Invitations.Services;
using FitLead.Application.Messenger.Chats.Access;
using FitLead.Application.Media.MediaAssets.Access;
using FitLead.Application.Media.MediaAssets.Registration;
using FitLead.Application.Media.Uploadcare;
using FitLead.Application.Modules.Exercises;
using FitLead.Application.Modules.TrainingPrograms;
using FitLead.Application.Modules.Users;
using FitLead.Application.Modules.Workouts;
using FitLead.Application.Notifications.Push;
using FitLead.Infrastructure.Invitations;
using FitLead.Application.Trainings.Exercises.Access;
using FitLead.Application.Trainings.TrainingPrograms.Access;
using FitLead.Application.Trainings.WorkoutLogs.Access;
using FitLead.Application.Trainings.Workouts.Access;
using FitLead.Application.Users.Access;
using FitLead.Infrastructure.Deletion;
using FitLead.Infrastructure.Identity;
using FitLead.Infrastructure.Modules.Exercises;
using FitLead.Infrastructure.Modules.TrainingPrograms;
using FitLead.Infrastructure.Modules.Users;
using FitLead.Infrastructure.Modules.Workouts;
using FitLead.Infrastructure.Media.Uploadcare;
using FitLead.Infrastructure.Media.MediaAssets;
using FitLead.Infrastructure.Notifications.Push;
using FitLead.Infrastructure.Outbox;
using FitLead.Infrastructure.Outbox.Handlers;
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
            services.AddScoped<IAssignedTrainingProgramRepository, AssignedTrainingProgramRepository>();
            services.AddScoped<IAssignedTrainingProgramReadRepository, AssignedTrainingProgramReadRepository>();
            services.AddScoped<ITrainerClientRepository, TrainerClientRepository>();
            services.AddScoped<ITrainerClientReadRepository, TrainerClientReadRepository>();
            services.AddScoped<ITrainerClientOverviewReadRepository, TrainerClientOverviewReadRepository>();
            services.AddScoped<ITrainerClientProgramsReadRepository, TrainerClientProgramsReadRepository>();
            services.AddScoped<ITrainerClientWorkoutLogsReadRepository, TrainerClientWorkoutLogsReadRepository>();
            services.AddScoped<ITrainerClientProgressReadRepository, TrainerClientProgressReadRepository>();
            services.AddScoped<IExerciseRepository, ExerciseRepository>();
            services.AddScoped<IExerciseReadRepository, ExerciseReadRepository>();
            services.AddScoped<IWorkoutRepository, WorkoutRepository>();
            services.AddScoped<IWorkoutReadRepository, WorkoutReadRepository>();
            services.AddScoped<IWorkoutLogRepository, WorkoutLogRepository>();
            services.AddScoped<IWorkoutLogAccessRepository, WorkoutLogAccessRepository>();
            services.AddScoped<IClientProfileRepository, ClientProfileRepository>();
            services.AddScoped<IClientBodyMetricEntryRepository, ClientBodyMetricEntryRepository>();
            services.AddScoped<IClientBodyMetricEntryReadRepository, ClientBodyMetricEntryReadRepository>();
            services.AddScoped<IClientProgressPhotoRepository, ClientProgressPhotoRepository>();
            services.AddScoped<IClientProgressPhotoReadRepository, ClientProgressPhotoReadRepository>();
            services.AddScoped<IInvitationRepository, InvitationRepository>();
            services.AddScoped<IInvitationReadRepository, InvitationReadRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IChatReadRepository, ChatReadRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IVideoReportRepository, VideoReportRepository>();
            services.AddScoped<IVideoReportReadRepository, VideoReportReadRepository>();
            services.AddScoped<IChatMessageReadRepository, ChatMessageReadRepository>();
            services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
            services.AddScoped<IMediaAssetReadRepository, MediaAssetReadRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationReadRepository, NotificationReadRepository>();
            services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
            services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
            services.AddScoped<IOutbox, Outbox.Outbox>();
            services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher>();
            services.AddSingleton<IOutboxMessageProcessor, OutboxMessageProcessor>();
            services.AddScoped<IOutboxMessageHandler, ChatMessageCreatedOutboxHandler>();
            services.AddScoped<IOutboxMessageHandler, VideoReportSubmittedNotificationOutboxHandler>();
            services.AddScoped<IOutboxMessageHandler, VideoReportReviewedNotificationOutboxHandler>();
            services.AddScoped<IOutboxMessageHandler, TrainingProgramAssignedNotificationOutboxHandler>();
            services.AddScoped<IOutboxMessageHandler, NotificationCreatedOutboxHandler>();
            services.AddScoped<IInvitationLinkService, InvitationLinkService>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DomainExceptionToResultBehavior<,>));
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<IClock, SystemClock>();
            services.AddDataProtection();
            services.Configure<DeletionTokenOptions>(
                configuration.GetSection(DeletionTokenOptions.SectionName));
            services
                .AddOptions<UploadcareOptions>()
                .Bind(configuration.GetSection(UploadcareOptions.SectionName))
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.PublicKey),
                    "Uploadcare public key is required")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.SecretKey),
                    "Uploadcare secret key is required")
                .Validate(
                    options => options.UploadSignatureLifetimeMinutes > 0,
                    "Uploadcare upload signature lifetime must be positive")
                .ValidateOnStart();
            services.Configure<MediaAssetRegistrationOptions>(
                configuration.GetSection(MediaAssetRegistrationOptions.SectionName));
            services
                .AddOptions<OutboxProcessorOptions>()
                .Bind(configuration.GetSection(OutboxProcessorOptions.SectionName))
                .Validate(
                    options => options.BatchSize is >= 1 and <= 100,
                    "Outbox processor batch size must be between 1 and 100")
                .Validate(
                    options => options.PollingIntervalSeconds > 0,
                    "Outbox processor polling interval must be positive")
                .Validate(
                    options => options.MaxAttempts > 0,
                    "Outbox processor max attempts must be positive")
                .ValidateOnStart();
            services
                .AddOptions<PushOptions>()
                .Bind(configuration.GetSection(PushOptions.SectionName));
            services.AddScoped<IPushVapidConfiguration>(provider =>
                provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PushOptions>>().Value);
            services.AddScoped<IWebPushSender, WebPushSender>();
            services.AddSingleton<IDeletionConfirmationTokenService, DataProtectionDeletionConfirmationTokenService>();
            services.AddScoped<ITokenHasher, TokenHasher>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IIdentityAccountService, IdentityAccountService>();
            services.AddScoped<IUserIdentityLinkWriter, UserIdentityLinkWriter>();
            services.AddScoped<IIdentityDomainUserLinkResolver, IdentityDomainUserLinkResolver>();
            services.AddScoped<IExercisesModule, ExercisesModule>();
            services.AddScoped<ITrainingProgramsModule, TrainingProgramsModule>();
            services.AddScoped<IUsersModule, UsersModule>();
            services.AddScoped<IWorkoutsModule, WorkoutsModule>();
            services.AddScoped<IWorkoutLoader, WorkoutLoader>();
            services.AddScoped<IWorkoutLogAccessLoader, WorkoutLogAccessLoader>();
            services.AddScoped<IExerciseLoader, ExerciseLoader>();
            services.AddScoped<ITrainingProgramLoader, TrainingProgramLoader>();
            services.AddScoped<IChatLoader, ChatLoader>();
            services.AddScoped<IMediaAssetLoader, MediaAssetLoader>();
            services.AddScoped<IMediaAssetRegistrationPolicy, MediaAssetRegistrationPolicy>();
            services.AddScoped<ICurrentUserLoader, CurrentUserLoader>();
            services.AddScoped<ITrainerClientAccessLoader, TrainerClientAccessLoader>();
            services.AddScoped<IUploadcareUploadSignatureService, UploadcareUploadSignatureService>();
            services.AddHttpClient<IUploadcareClient, UploadcareClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.uploadcare.com");
            });
            services.AddHostedService<OutboxProcessor>();

            return services;
        }
    }
}
