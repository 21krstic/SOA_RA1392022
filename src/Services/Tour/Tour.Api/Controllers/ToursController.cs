using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourService.Data;
using TourService.Dtos;
using TourService.Models;

namespace TourService.Controllers;

[ApiController]
[Route("api/tours")]
public class ToursController : ControllerBase
{
    private readonly ToursRepository _tours;

    public ToursController(ToursRepository tours)
    {
        _tours = tours;
    }

    [Authorize(Roles = "Guide")]
    [HttpPost]
    public async Task<ActionResult<Tour>> Create(CreateTourRequest request)
    {
        var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var tour = new Tour
        {
            AuthorId = authorId,
            Name = request.Name,
            Description = request.Description,
            Difficulty = request.Difficulty,
            Tags = request.Tags,
            Status = TourStatus.Draft,
            Price = 0
        };

        await _tours.CreateAsync(tour);
        return Ok(tour);
    }

    [Authorize(Roles = "Guide")]
    [HttpGet("mine")]
    public async Task<ActionResult<List<Tour>>> GetMine()
    {
        var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _tours.GetByAuthorAsync(authorId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tour>> GetById(string id)
    {
        var tour = await _tours.GetByIdAsync(id);
        return tour is null ? NotFound() : Ok(tour);
    }

    [Authorize(Roles = "Guide")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, UpdateTourStatusRequest request)
    {
        var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tour = await _tours.GetByIdAsync(id);
        if (tour is null) return NotFound();
        if (tour.AuthorId != authorId) return Forbid();

        await _tours.UpdateStatusAsync(id, request.Status);
        return NoContent();
    }
}
