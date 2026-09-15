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
    private readonly KeyPointsRepository _keyPoints;

    public ToursController(ToursRepository tours, KeyPointsRepository keyPoints)
    {
        _tours = tours;
        _keyPoints = keyPoints;
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

        var isValidTransition = (tour.Status, request.Status) is
            (TourStatus.Draft, TourStatus.Published) or
            (TourStatus.Published, TourStatus.Archived);
        if (!isValidTransition)
            return BadRequest($"Cannot transition a tour from {tour.Status} to {request.Status}.");

        if (request.Status == TourStatus.Published)
        {
            var keyPoints = await _keyPoints.GetByTourAsync(id);
            var hasStart = keyPoints.Any(k => k.Type == KeyPointType.Start);
            var hasEnd = keyPoints.Any(k => k.Type == KeyPointType.End);
            if (!hasStart || !hasEnd)
                return BadRequest("A tour needs both a Start and an End key point before it can be published.");
        }

        await _tours.UpdateStatusAsync(id, request.Status);
        return NoContent();
    }

    [Authorize(Roles = "Guide")]
    [HttpPut("{id}/price")]
    public async Task<IActionResult> UpdatePrice(string id, UpdateTourPriceRequest request)
    {
        if (request.Price < 0) return BadRequest("Price cannot be negative.");

        var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tour = await _tours.GetByIdAsync(id);
        if (tour is null) return NotFound();
        if (tour.AuthorId != authorId) return Forbid();

        await _tours.UpdatePriceAsync(id, request.Price);
        return NoContent();
    }
}
