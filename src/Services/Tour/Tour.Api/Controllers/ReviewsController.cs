using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourService.Data;
using TourService.Dtos;
using TourService.Models;
using TourService.Services;

namespace TourService.Controllers;

[ApiController]
[Route("api/tours/{tourId}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly ToursRepository _tours;
    private readonly ReviewsRepository _reviews;
    private readonly PurchaseServiceClient _purchaseClient;

    public ReviewsController(ToursRepository tours, ReviewsRepository reviews, PurchaseServiceClient purchaseClient)
    {
        _tours = tours;
        _reviews = reviews;
        _purchaseClient = purchaseClient;
    }

    // A tour can only be reviewed by a tourist who has purchased it (requirement 16).
    [Authorize(Roles = "Tourist")]
    [HttpPost]
    public async Task<ActionResult<Review>> Create(string tourId, CreateReviewRequest request)
    {
        if (request.Rating is < 1 or > 5) return BadRequest("Rating must be between 1 and 5.");

        var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tour = await _tours.GetByIdAsync(tourId);
        if (tour is null) return NotFound("Tour not found.");

        if (!await _purchaseClient.IsPurchasedAsync(touristId, tourId))
            return Forbid();

        var review = new Review
        {
            TourId = tourId,
            TouristId = touristId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _reviews.CreateAsync(review);
        return Ok(review);
    }

    // Reviews are visible pre-purchase (requirement 16), so this is public.
    [HttpGet]
    public async Task<ActionResult<List<Review>>> GetAll(string tourId) =>
        Ok(await _reviews.GetByTourAsync(tourId));
}
