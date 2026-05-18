using FitLead.Api.Common.Results;
using FitLead.Api.Chats.Contracts;
using FitLead.Application.Messenger.ChatMessages.Commands;
using FitLead.Application.Messenger.ChatMessages.Queries;
using FitLead.Application.Messenger.Chats.Commands;
using FitLead.Application.Messenger.Chats.Queries;
using FitLead.Application.Messenger.VideoReports.Commands;
using FitLead.Application.Messenger.VideoReports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Chats
{
    [ApiController]
    [Route("api/chats")]
    public sealed class ChatsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetChats(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetChatsQuery(),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize]
        [HttpGet("{chatId:guid}")]
        public async Task<IActionResult> GetChat(
            Guid chatId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetChatDetailsQuery(chatId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [ValidateAntiForgeryToken]
        [HttpPost("with-client/{clientId:guid}")]
        public async Task<IActionResult> GetOrCreateWithClient(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetOrCreateChatWithClientCommand(clientId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "ClientOnly")]
        [ValidateAntiForgeryToken]
        [HttpPost("with-trainer/{trainerId:guid}")]
        public async Task<IActionResult> GetOrCreateWithTrainer(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetOrCreateChatWithTrainerCommand(trainerId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("{chatId:guid}/messages")]
        public async Task<IActionResult> SendTextMessage(
            Guid chatId,
            [FromBody] SendTextMessageRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new SendTextMessageCommand(chatId, request.Text),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "ClientOnly")]
        [ValidateAntiForgeryToken]
        [HttpPost("{chatId:guid}/video-reports")]
        public async Task<IActionResult> CreateVideoReport(
            Guid chatId,
            [FromBody] CreateVideoReportRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateVideoReportCommand(
                    chatId,
                    request.Title,
                    request.Description,
                    request.MediaAssetIds),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize]
        [HttpGet("{chatId:guid}/video-reports/{reportId:guid}")]
        public async Task<IActionResult> GetVideoReport(
            Guid chatId,
            Guid reportId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetVideoReportDetailsQuery(chatId, reportId),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize(Policy = "TrainerOnly")]
        [ValidateAntiForgeryToken]
        [HttpPost("{chatId:guid}/video-reports/{reportId:guid}/feedback")]
        public async Task<IActionResult> SubmitVideoReportFeedback(
            Guid chatId,
            Guid reportId,
            [FromBody] SubmitVideoReportFeedbackRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new SubmitVideoReportFeedbackCommand(
                    chatId,
                    reportId,
                    request.Text),
                cancellationToken);

            return result.ToActionResult(this);
        }

        [Authorize]
        [HttpGet("{chatId:guid}/messages")]
        public async Task<IActionResult> GetMessages(
            Guid chatId,
            [FromQuery] int? limit,
            [FromQuery] DateTime? beforeCreatedAtUtc,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetChatMessagesQuery(chatId, limit, beforeCreatedAtUtc),
                cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
