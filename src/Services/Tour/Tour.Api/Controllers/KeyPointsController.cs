using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourService.Data;
using TourService.Dtos;
using TourService.Models;

namespace TourService.Controllers;

[ApiController]
[Route("api/tours/{tourId}/keypoints")]
public class KeyPointsController : ControllerBase
{
    private readonly ToursRepository _tours;
    private readonly KeyPointsRepository _keyPoints;

    public KeyPointsController(ToursRepository tours, KeyPointsRepository keyPoints)
    {
        _tours = tours;
        _keyPoints = keyPoints;
    }

    [Authorize(Roles = "Guide")]
    [HttpPost]
    public async Task<ActionResult<KeyPoint>> Create(string tourId, CreateKeyPointRequest request)
    {
        var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tour = await _tours.GetByIdAsync(tourId);
        if (tour is null) return NotFound("Tour not found.");
        if (tour.AuthorId != authorId) return Forbid();

        var existing = await _keyPoints.GetByTourAsync(tourId);
        if (existing.Any(k => k.Type == request.Type))
            return Conflict($"A {request.Type} key point already exists for this tour.");
        if (existing.Count >= 2)
            return Conflict("A tour can only have a Start and an End key point.");

        var keyPoint = new KeyPoint
        {
            TourId = tourId,
            Type = request.Type,
            Name = request.Name,
            Description = request.Description,
            ImagePath = request.ImagePath,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        await _keyPoints.CreateAsync(keyPoint);
        return Ok(keyPoint);
    }

    [HttpGet]
    public async Task<ActionResult<List<KeyPoint>>> GetAll(string tourId) =>
        Ok(await _keyPoints.GetByTourAsync(tourId));
}
