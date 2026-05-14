using FitLead.Domain.Messenger.Chats;
using FitLead.Infrastructure.Persistence.Models;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;

namespace FitLead.IntegrationTests.Features.Messenger;

public abstract class MessengerTestBase : IntegrationTestBase
{
    protected readonly TestDb Db;
    protected readonly TestUsers Users;
    protected readonly TestApiClients Api;

    protected MessengerTestBase(IntegrationTestFixture fixture) : base(fixture)
    {
        Db = new TestDb(fixture);
        Users = new TestUsers(fixture, Db);
        Api = new TestApiClients(fixture);
    }

    protected async Task CreateRelationshipAsync(Guid trainerId, Guid clientId)
    {
        await Db.ExecuteAsync(async context =>
        {
            await context.TrainerClients.AddAsync(new TrainerClient(trainerId, clientId));
            await context.SaveChangesAsync();
        });
    }

    protected async Task<Chat> CreateChatAsync(Guid trainerId, Guid clientId)
    {
        var chat = Chat.Create(trainerId, clientId, DateTime.UtcNow).Value;

        await Db.ExecuteAsync(async context =>
        {
            await context.Chats.AddAsync(chat);
            await context.SaveChangesAsync();
        });

        return chat;
    }
}
