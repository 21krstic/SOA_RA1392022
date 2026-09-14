using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Purchase.Api.Data;

namespace Purchase.Api.Controllers;

[ApiController]
[Route("api/purchases")]
public class PurchaseTokensController : ControllerBase
{
    private readonly PurchaseTokensRepository _tokens;

    public PurchaseTokensController(PurchaseTokensRepository tokens)
    {
        _tokens = tokens;
    }

    [Authorize]
    [HttpGet("{touristId}")]
    public async Task<IActionResult> GetPurchases(string touristId)
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) != touristId) return Forbid();
        return Ok(await _tokens.GetByTouristAsync(touristId));
    }

    [HttpGet("{touristId}/is-purchased/{tourId}")]
    public async Task<ActionResult<bool>> IsPurchased(string touristId, string tourId) =>
        Ok(await _tokens.IsPurchasedAsync(touristId, tourId));
}
