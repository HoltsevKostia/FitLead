using System.Net;
using FitLead.Application.Messenger.Chats.Queries;
using FitLead.Domain.Messenger.Chats;
using FitLead.Infrastructure.Persistence.Models;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ChatDetailsTests : IntegrationTestBase
{
    private readonly TestDb _db;
    private readonly TestUsers _users;
    private readonly TestApiClients _api;

    public ChatDetailsTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _db = new TestDb(fixture);
        _users = new TestUsers(fixture, _db);
        _api = new TestApiClients(fixture);
    }

    [Fact]
    public async Task TrainerGetChatDetails_ShouldReturnOwnClientChat()
    {
        var trainer = await _users.RegisterTrainerAsync("chat-details-trainer");
        var client = await _users.RegisterClientAsync("chat-details-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var chatsClient = await _api.ChatsAsync(trainer.Auth);

        var response = await chatsClient.GetChatAsync(chat.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var details = await response.ReadRequiredJsonAsync<ChatDetailsDto>();
        details.Id.Should().Be(chat.Id);
        details.TrainerId.Should().Be(trainer.Id);
        details.ClientId.Should().Be(client.Id);
        details.TrainerName.Should().Be("Test Trainer");
        details.ClientName.Should().Be("Test Client");
        details.LastMessageAtUtc.Should().BeNull();
    }

    private async Task CreateRelationshipAsync(Guid trainerId, Guid clientId)
    {
        await _db.ExecuteAsync(async context =>
        {
            await context.TrainerClients.AddAsync(new TrainerClient(trainerId, clientId));
            await context.SaveChangesAsync();
        });
    }

    private async Task<Chat> CreateChatAsync(Guid trainerId, Guid clientId)
    {
        var chat = Chat.Create(trainerId, clientId, DateTime.UtcNow).Value;

        await _db.ExecuteAsync(async context =>
        {
            await context.Chats.AddAsync(chat);
            await context.SaveChangesAsync();
        });

        return chat;
    }
}
