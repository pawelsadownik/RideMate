using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;

namespace RideMate.Api.Controllers;

[ApiController]
[Route("api/subscription")]
[Authorize]
public sealed class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService) => _subscriptionService = subscriptionService;

    private Guid UserId => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var sub = await _subscriptionService.GetSubscriptionAsync(UserId, ct);
        return Ok(sub);
    }

    [HttpPost("dev/premium")]
    public async Task<IActionResult> SetDevPremium([FromBody] DevTogglePremiumRequest request, CancellationToken ct)
    {
        // Weryfikacja produkcji — w rzeczywistej produkcji IEnvironment.IsDevelopment() powinno blokować.
        // Tutaj warunek w serwisie.
        try
        {
            await _subscriptionService.SetDevPremiumAsync(UserId, request.IsPremium, request.IsProduction, ct);
            return Ok(new { message = "Tryb deweloperski zaktualizowany." });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
