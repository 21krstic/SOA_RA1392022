using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourService.Data;
using TourService.Dtos;

namespace TourService.Controllers;

[Authorize]
[ApiController]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private readonly PositionsRepository _positions;

    public PositionsController(PositionsRepository positions)
    {
        _positions = positions;
    }

    [HttpGet("{touristId}")]
    public async Task<IActionResult> Get(string touristId)
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) != touristId) return Forbid();

        var position = await _positions.GetAsync(touristId);
        return position is null ? NotFound() : Ok(position);
    }

    [HttpPut("{touristId}")]
    public async Task<IActionResult> Set(string touristId, SetPositionRequest request)
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) != touristId) return Forbid();

        await _positions.SetAsync(touristId, request.Latitude, request.Longitude);
        return NoContent();
    }
}
