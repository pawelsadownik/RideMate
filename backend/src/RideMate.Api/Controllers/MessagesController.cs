using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;

namespace RideMate.Api.Controllers;

[ApiController]
[Route("api/chat/conversations/{conversationId}/messages")]
[Authorize]
public sealed class MessagesController : ControllerBase
{
    private readonly IChatService _chatService;

    public MessagesController(IChatService chatService) => _chatService = chatService;

    private Guid UserId => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> Get(Guid conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var messages = await _chatService.GetMessagesAsync(UserId, conversationId, page, pageSize, ct);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> Send(Guid conversationId, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        try
        {
            var message = await _chatService.SendMessageAsync(UserId, conversationId, request, ct);
            return Ok(message);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("read")]
    public async Task<IActionResult> MarkRead(Guid conversationId, CancellationToken ct)
    {
        await _chatService.MarkConversationReadAsync(UserId, conversationId, ct);
        return NoContent();
    }
}
