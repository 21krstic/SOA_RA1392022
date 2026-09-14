using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourService.Data;
using TourService.Dtos;
using TourService.Models;
using TourService.Services;

namespace TourService.Controllers;

[ApiController]
[Route("api/tours/{tourId}/keypoints")]
public class KeyPointsController : ControllerBase
{
    private readonly ToursRepository _tours;
    private readonly KeyPointsRepository _keyPoints;
    private readonly PurchaseServiceClient _purchaseClient;

    public KeyPointsController(ToursRepository tours, KeyPointsRepository keyPoints, PurchaseServiceClient purchaseClient)
    {
        _tours = tours;
        _keyPoints = keyPoints;
        _purchaseClient = purchaseClient;
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

    // Unpurchased tourists only see the starting point; the author and anyone
    // who purchased the tour see every key point (requirement 16).
    [HttpGet]
    public async Task<ActionResult<List<KeyPoint>>> GetAll(string tourId)
    {
        var tour = await _tours.GetByIdAsync(tourId);
        if (tour is null) return NotFound();

        var keyPoints = await _keyPoints.GetByTourAsync(tourId);

        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAuthor = callerId is not null && callerId == tour.AuthorId;
        var isPurchased = !isAuthor && callerId is not null && await _purchaseClient.IsPurchasedAsync(callerId, tourId);

        if (isAuthor || isPurchased) return Ok(keyPoints);

        return Ok(keyPoints.Where(k => k.Type == KeyPointType.Start).ToList());
    }
}
