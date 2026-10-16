using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMate.Application.Interfaces;

namespace RideMate.Api.Controllers;

[ApiController]
[Route("api/chat/conversations")]
[Authorize]
public sealed class ConversationsController : ControllerBase
{
    private readonly IChatService _chatService;

    public ConversationsController(IChatService chatService) => _chatService = chatService;

    private Guid UserId => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var conversations = await _chatService.GetConversationsAsync(UserId, ct);
        return Ok(conversations);
    }
}
