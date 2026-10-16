using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;

namespace RideMate.Api.Controllers;

[ApiController]
[Route("api/profiles")]
[Authorize]
public sealed class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfilesController(IProfileService profileService) => _profileService = profileService;

    private Guid UserId => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var profile = await _profileService.GetMyProfileAsync(UserId, ct);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        try
        {
            var profile = await _profileService.UpdateProfileAsync(UserId, request, ct);
            return Ok(profile);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("me/preferences")]
    public async Task<IActionResult> UpdateRidingPreferences([FromBody] UpdateRidingPreferencesRequest request, CancellationToken ct)
    {
        try
        {
            var prefs = await _profileService.UpdateRidingPreferencesAsync(UserId, request, ct);
            return Ok(prefs);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("me/photos")]
    public async Task<IActionResult> AddPhoto([FromBody] AddPhotoRequest request, CancellationToken ct)
    {
        try
        {
            await _profileService.AddPhotoAsync(UserId, request, ct);
            return Ok(new { message = "Zdjęcie dodane." });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("me/photos/{photoId}")]
    public async Task<IActionResult> RemovePhoto(Guid photoId, CancellationToken ct)
    {
        try
        {
            await _profileService.RemovePhotoAsync(UserId, photoId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("me/photos/{photoId}/primary")]
    public async Task<IActionResult> SetPrimaryPhoto(Guid photoId, CancellationToken ct)
    {
        try
        {
            await _profileService.SetPrimaryPhotoAsync(UserId, photoId, ct);
            return Ok(new { message = "Ustawiono zdjęcie główne." });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetProfileByUserId(Guid userId, CancellationToken ct)
    {
        try
        {
            var profile = await _profileService.GetProfileByUserIdAsync(userId, ct);
            return Ok(profile);
        }
        catch (InvalidOperationException ex) { return NotFound(new { error = ex.Message }); }
    }
}
