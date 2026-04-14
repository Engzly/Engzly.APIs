using Engzly.Application.Features.Chat.Commands;
using Engzly.Application.Features.Chat.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    [Authorize]
    public sealed class ChatController(ISender _mediator) : BaseApiController
    {
        [HttpGet("conversations")]
        public async Task<IActionResult> GetMyConversations()
            => Resolve(await _mediator.Send(new GetMyConversationsQuery()));

        [HttpPost("conversations")]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationCommand command)
            => Resolve(await _mediator.Send(command));

        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(
            [FromRoute] string conversationId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
            => Resolve(await _mediator.Send(new GetConversationMessagesQuery(conversationId, page, pageSize)));

        [HttpPost("messages/text")]
        public async Task<IActionResult> SendText([FromBody] SendTextMessageCommand command)
            => Resolve(await _mediator.Send(command));

        [HttpPost("messages/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendImage(
            [FromForm] string conversationId,
            IFormFile image)
            => Resolve(await _mediator.Send(new SendImageMessageCommand(conversationId, image)));

        [HttpPost("messages/location")]
        public async Task<IActionResult> SendLocation([FromBody] SendLocationMessageCommand command)
            => Resolve(await _mediator.Send(command));

        [HttpPatch("conversations/{conversationId}/read")]
        public async Task<IActionResult> MarkRead([FromRoute] string conversationId)
            => Resolve(await _mediator.Send(new MarkConversationReadCommand(conversationId)));

        [HttpPost("bot/ask")]
        public async Task<IActionResult> AskBot([FromBody] AskChatBotCommand command)
            => Resolve(await _mediator.Send(command));
    }
}
