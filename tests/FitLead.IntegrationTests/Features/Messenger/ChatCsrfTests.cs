using System.Net;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ChatCsrfTests : MessengerTestBase
{
    public ChatCsrfTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetOrCreateWithClient_WithoutCsrf_ShouldReturnBadRequest()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-csrf-create-trainer");
        var client = await Users.RegisterClientAsync("chat-csrf-create-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chatsClient = await Api.ChatsAsync(trainer.Auth);

        var response = await chatsClient.GetOrCreateWithClientAsync(
            client.Id,
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendTextMessage_WithoutCsrf_ShouldReturnBadRequest()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-csrf-send-trainer");
        var client = await Users.RegisterClientAsync("chat-csrf-send-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var chatsClient = await Api.ChatsAsync(trainer.Auth);

        var response = await chatsClient.SendTextMessageAsync(
            chat.Id,
            "Привіт",
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
