using System.Net;
using FitLead.Application.Messenger.Chats.Queries;
using FitLead.Infrastructure.Persistence.Models;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ChatGetOrCreateTests : IntegrationTestBase
{
    private readonly TestDb _db;
    private readonly TestUsers _users;
    private readonly TestApiClients _api;

    public ChatGetOrCreateTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _db = new TestDb(fixture);
        _users = new TestUsers(fixture, _db);
        _api = new TestApiClients(fixture);
    }

    [Fact]
    public async Task TrainerGetOrCreateWithOwnClient_ShouldCreateChat()
    {
        var trainer = await _users.RegisterTrainerAsync("chat-trainer-create");
        var client = await _users.RegisterClientAsync("chat-client-create");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);
        var chats = await _api.ChatsAsync(trainer.Auth);

        var response = await chats.GetOrCreateWithClientAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var chat = await response.ReadRequiredJsonAsync<ChatDto>();
        chat.Id.Should().NotBeEmpty();
        chat.TrainerId.Should().Be(trainer.Id);
        chat.ClientId.Should().Be(client.Id);
        chat.LastMessageAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task TrainerGetOrCreateSecondCall_ShouldReturnSameChat()
    {
        var trainer = await _users.RegisterTrainerAsync("chat-trainer-idempotent");
        var client = await _users.RegisterClientAsync("chat-client-idempotent");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);
        var chats = await _api.ChatsAsync(trainer.Auth);

        var firstResponse = await chats.GetOrCreateWithClientAsync(client.Id);
        var secondResponse = await chats.GetOrCreateWithClientAsync(client.Id);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var first = await firstResponse.ReadRequiredJsonAsync<ChatDto>();
        var second = await secondResponse.ReadRequiredJsonAsync<ChatDto>();
        second.Id.Should().Be(first.Id);

        var chatCount = await _db.QueryAsync(context =>
            context.Chats.CountAsync(x =>
                x.TrainerId == trainer.Id &&
                x.ClientId == client.Id));
        chatCount.Should().Be(1);
    }

    [Fact]
    public async Task TrainerGetOrCreateWithUnrelatedClient_ShouldReturnNotFound()
    {
        var trainer = await _users.RegisterTrainerAsync("chat-trainer-unrelated");
        var client = await _users.RegisterClientAsync("chat-client-unrelated");
        var chats = await _api.ChatsAsync(trainer.Auth);

        var response = await chats.GetOrCreateWithClientAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("chat.not_found");
    }

    [Fact]
    public async Task ClientGetOrCreateWithOwnTrainer_ShouldCreateChat()
    {
        var trainer = await _users.RegisterTrainerAsync("chat-trainer-client-open");
        var client = await _users.RegisterClientAsync("chat-client-open");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);
        var chats = await _api.ChatsAsync(client.Auth);

        var response = await chats.GetOrCreateWithTrainerAsync(trainer.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var chat = await response.ReadRequiredJsonAsync<ChatDto>();
        chat.TrainerId.Should().Be(trainer.Id);
        chat.ClientId.Should().Be(client.Id);
    }

    [Fact]
    public async Task GetOrCreateWithoutCsrf_ShouldReturnBadRequest()
    {
        var trainer = await _users.RegisterTrainerAsync("chat-trainer-csrf");
        var client = await _users.RegisterClientAsync("chat-client-csrf");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);
        var chats = await _api.ChatsAsync(trainer.Auth);

        var response = await chats.GetOrCreateWithClientAsync(
            client.Id,
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task CreateTrainerClientRelationshipAsync(Guid trainerId, Guid clientId)
    {
        await _db.ExecuteAsync(async context =>
        {
            await context.TrainerClients.AddAsync(new TrainerClient(trainerId, clientId));
            await context.SaveChangesAsync();
        });
    }
}
