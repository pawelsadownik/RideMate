using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMate.Application.Interfaces;

namespace RideMate.Api.Controllers;

[ApiController]
[Route("api/push-tokens")]
[Authorize]
public sealed class PushTokensController : ControllerBase
{
    private readonly IPushTokenService _pushTokenService;

    public PushTokensController(IPushTokenService pushTokenService) => _pushTokenService = pushTokenService;

    private Guid UserId => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertPushTokenRequest request, CancellationToken ct)
    {
        await _pushTokenService.UpsertTokenAsync(UserId, request.Token, request.Platform, ct);
        return NoContent();
    }
}

public sealed class UpsertPushTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}
