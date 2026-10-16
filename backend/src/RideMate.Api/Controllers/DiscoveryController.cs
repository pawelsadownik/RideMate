using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;

namespace RideMate.Api.Controllers;

[ApiController]
[Route("api/discovery")]
[Authorize]
public sealed class DiscoveryController : ControllerBase
{
    private readonly IDiscoveryService _discoveryService;

    public DiscoveryController(IDiscoveryService discoveryService) => _discoveryService = discoveryService;

    private Guid UserId => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetProfiles(
        [FromQuery] string? activityType,
        [FromQuery] double? maxDistanceKm,
        [FromQuery] int? minAge,
        [FromQuery] int? maxAge,
        [FromQuery] string? lookingFor,
        [FromQuery] string? ridingStyle,
        CancellationToken ct)
    {
        var filters = new DiscoveryFilters(activityType, maxDistanceKm, minAge, maxAge, lookingFor, ridingStyle);
        var profiles = await _discoveryService.GetDiscoveryProfilesAsync(UserId, filters, ct);
        return Ok(profiles);
    }

    [HttpGet("likes/remaining")]
    public async Task<IActionResult> RemainingLikes(CancellationToken ct)
    {
        var remaining = await _discoveryService.RemainingLikesTodayAsync(UserId, ct);
        return Ok(new { remaining });
    }

    [HttpPost("like")]
    public async Task<IActionResult> LikeOrPass([FromBody] LikeActionRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _discoveryService.LikeOrPassAsync(UserId, request, ct);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
