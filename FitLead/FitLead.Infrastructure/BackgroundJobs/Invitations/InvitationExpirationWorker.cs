using FitLead.Application.Invitations.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace FitLead.Infrastructure.BackgroundJobs.Invitations
{
    public sealed class InvitationExpirationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public InvitationExpirationWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(
                    new ExpireInvitationsCommand(DateTime.UtcNow),
                    stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

}
