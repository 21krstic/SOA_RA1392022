using Microsoft.AspNetCore.Mvc;
using TourService.Data;
using TourService.Dtos;

namespace TourService.Controllers;

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
        var position = await _positions.GetAsync(touristId);
        return position is null ? NotFound() : Ok(position);
    }

    [HttpPut("{touristId}")]
    public async Task<IActionResult> Set(string touristId, SetPositionRequest request)
    {
        await _positions.SetAsync(touristId, request.Latitude, request.Longitude);
        return NoContent();
    }
}
