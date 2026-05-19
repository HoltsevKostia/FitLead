using System.Net;
using FitLead.Application.Messenger.Chats.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ChatDetailsTests : MessengerTestBase
{
    public ChatDetailsTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task TrainerGetChatDetails_ShouldReturnOwnClientChat()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-details-trainer");
        var client = await Users.RegisterClientAsync("chat-details-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var chatsClient = await Api.ChatsAsync(trainer.Auth);

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

}
