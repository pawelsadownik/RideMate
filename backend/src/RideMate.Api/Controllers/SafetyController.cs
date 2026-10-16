using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;
using RideMate.Domain;

namespace RideMate.Api.Controllers;

[ApiController]
[Route("api/safety")]
[Authorize]
public sealed class SafetyController : ControllerBase
{
    private readonly ISafetyService _safetyService;

    public SafetyController(ISafetyService safetyService) => _safetyService = safetyService;

    private Guid UserId => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpPost("block")]
    public async Task<IActionResult> Block([FromBody] BlockRequest request, CancellationToken ct)
    {
        try
        {
            await _safetyService.BlockUserAsync(UserId, request.BlockedUserId, ct);
            return Ok(new { message = "Użytkownik został zablokowany." });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("block/{blockedUserId}")]
    public async Task<IActionResult> Unblock(Guid blockedUserId, CancellationToken ct)
    {
        try
        {
            await _safetyService.UnblockUserAsync(UserId, blockedUserId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("blocked")]
    public async Task<IActionResult> GetBlocked(CancellationToken ct)
    {
        var ids = await _safetyService.GetBlockedUserIdsAsync(UserId, ct);
        return Ok(ids);
    }

    [HttpPost("report")]
    public async Task<IActionResult> Report([FromBody] ReportRequest request, CancellationToken ct)
    {
        try
        {
            var report = await _safetyService.CreateReportAsync(UserId, request, ct);
            return Ok(report);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // Panel moderacji (dla admina — w MVP prosty endpoint, w produkcji z rolą admin)
    [HttpGet("reports/open")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetOpenReports(CancellationToken ct)
    {
        var reports = await _safetyService.GetOpenReportsAsync(ct);
        return Ok(reports);
    }

    [HttpPost("reports/{reportId}/status")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> SetReportStatus(Guid reportId, [FromBody] SetReportStatusRequest request, CancellationToken ct)
    {
        try
        {
            await _safetyService.SetReportStatusAsync(reportId, request.Status, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public sealed class SetReportStatusRequest
{
    public ReportStatus Status { get; set; }
}
