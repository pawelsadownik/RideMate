using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMate.Application.Interfaces;

namespace RideMate.Api.Controllers;

[ApiController]
[Route("api/matches")]
[Authorize]
public sealed class MatchesController : ControllerBase
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService) => _matchService = matchService;

    private Guid UserId => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetMatches(CancellationToken ct)
    {
        var matches = await _matchService.GetMatchesAsync(UserId, ct);
        return Ok(matches);
    }

    [HttpDelete("{matchId}")]
    public async Task<IActionResult> Unmatch(Guid matchId, CancellationToken ct)
    {
        try
        {
            await _matchService.UnmatchAsync(UserId, matchId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
