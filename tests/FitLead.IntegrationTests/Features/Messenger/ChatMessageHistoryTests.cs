using System.Net;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Messenger;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ChatMessageHistoryTests : MessengerTestBase
{
    public ChatMessageHistoryTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetMessages_ShouldReturnLatestMessagesInAscendingOrder()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-history-trainer");
        var client = await Users.RegisterClientAsync("chat-history-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var now = DateTime.UtcNow;
        var first = await CreateTextMessageAsync(chat, trainer.Id, "first", now.AddMinutes(-3));
        var second = await CreateTextMessageAsync(chat, client.Id, "second", now.AddMinutes(-2));
        var third = await CreateTextMessageAsync(chat, trainer.Id, "third", now.AddMinutes(-1));
        var chatsClient = await Api.ChatsAsync(trainer.Auth);

        var response = await chatsClient.GetMessagesAsync(chat.Id, limit: 2);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.ReadRequiredJsonAsync<ChatMessageHistoryDto>();
        history.HasMore.Should().BeTrue();
        history.Items.Select(x => x.Id).Should().Equal(second.Id, third.Id);
        history.Items.Select(x => x.Text).Should().Equal("second", "third");
        history.Items[0].SenderName.Should().Be("Test Client");
        history.Items[1].SenderName.Should().Be("Test Trainer");
        history.Items.Should().NotContain(x => x.Id == first.Id);
    }

    [Fact]
    public async Task GetMessages_WithBeforeCreatedAtUtc_ShouldReturnOlderMessages()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-history-cursor-trainer");
        var client = await Users.RegisterClientAsync("chat-history-cursor-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        var now = DateTime.UtcNow;
        var first = await CreateTextMessageAsync(chat, trainer.Id, "first", now.AddMinutes(-3));
        var second = await CreateTextMessageAsync(chat, client.Id, "second", now.AddMinutes(-2));
        var third = await CreateTextMessageAsync(chat, trainer.Id, "third", now.AddMinutes(-1));
        var chatsClient = await Api.ChatsAsync(client.Auth);

        var response = await chatsClient.GetMessagesAsync(
            chat.Id,
            limit: 10,
            beforeCreatedAtUtc: third.CreatedAtUtc);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.ReadRequiredJsonAsync<ChatMessageHistoryDto>();
        history.HasMore.Should().BeFalse();
        history.Items.Select(x => x.Id).Should().Equal(first.Id, second.Id);
        history.Items.Should().NotContain(x => x.Id == third.Id);
    }

    [Fact]
    public async Task GetMessages_ForUnrelatedUser_ShouldReturnNotFound()
    {
        var trainer = await Users.RegisterTrainerAsync("chat-history-owner-trainer");
        var client = await Users.RegisterClientAsync("chat-history-owner-client");
        var unrelatedClient = await Users.RegisterClientAsync("chat-history-unrelated-client");
        await CreateRelationshipAsync(trainer.Id, client.Id);
        var chat = await CreateChatAsync(trainer.Id, client.Id);
        await CreateTextMessageAsync(chat, trainer.Id, "first", DateTime.UtcNow);
        var chatsClient = await Api.ChatsAsync(unrelatedClient.Auth);

        var response = await chatsClient.GetMessagesAsync(chat.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("chat.not_found");
    }
}
