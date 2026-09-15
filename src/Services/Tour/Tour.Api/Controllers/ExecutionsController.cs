using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourService.Data;
using TourService.Dtos;
using TourService.Models;
using TourService.Services;

namespace TourService.Controllers;

[Authorize(Roles = "Tourist")]
[ApiController]
[Route("api/executions")]
public class ExecutionsController : ControllerBase
{
    private const double ProximityThresholdMeters = 50;

    private readonly ExecutionsRepository _executions;
    private readonly ToursRepository _tours;
    private readonly KeyPointsRepository _keyPoints;
    private readonly PurchaseServiceClient _purchaseClient;

    public ExecutionsController(
        ExecutionsRepository executions,
        ToursRepository tours,
        KeyPointsRepository keyPoints,
        PurchaseServiceClient purchaseClient)
    {
        _executions = executions;
        _tours = tours;
        _keyPoints = keyPoints;
        _purchaseClient = purchaseClient;
    }

    [HttpPost]
    public async Task<ActionResult<TourExecution>> Start(StartExecutionRequest request)
    {
        var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var tour = await _tours.GetByIdAsync(request.TourId);
        if (tour is null) return NotFound("Tour not found.");
        if (tour.Status == TourStatus.Draft) return BadRequest("Draft tours cannot be started.");

        if (!await _purchaseClient.IsPurchasedAsync(touristId, request.TourId))
            return Forbid();

        var existingActive = await _executions.GetActiveAsync(touristId, request.TourId);
        if (existingActive is not null) return Ok(existingActive);

        var execution = new TourExecution
        {
            TouristId = touristId,
            TourId = request.TourId
        };
        await _executions.CreateAsync(execution);
        return Ok(execution);
    }

    [HttpPost("{id}/check-progress")]
    public async Task<ActionResult<TourExecution>> CheckProgress(string id, CheckProgressRequest request)
    {
        var execution = await _executions.GetByIdAsync(id);
        if (execution is null) return NotFound();
        if (execution.TouristId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
        if (execution.Status != TourExecutionStatus.Active) return BadRequest("Execution is not active.");

        var keyPoints = await _keyPoints.GetByTourAsync(execution.TourId);
        var completedIds = execution.CompletedKeyPoints.Select(c => c.KeyPointId).ToHashSet();

        foreach (var keyPoint in keyPoints.Where(k => !completedIds.Contains(k.Id)))
        {
            var distance = GeoUtils.DistanceMeters(request.Latitude, request.Longitude, keyPoint.Latitude, keyPoint.Longitude);
            if (distance <= ProximityThresholdMeters)
            {
                execution.CompletedKeyPoints.Add(new CompletedKeyPoint { KeyPointId = keyPoint.Id, CompletedAt = DateTime.UtcNow });
            }
        }

        execution.LastActivity = DateTime.UtcNow;
        await _executions.ReplaceAsync(execution);
        return Ok(execution);
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<TourExecution>> Complete(string id) => await Finish(id, TourExecutionStatus.Completed);

    [HttpPost("{id}/abandon")]
    public async Task<ActionResult<TourExecution>> Abandon(string id) => await Finish(id, TourExecutionStatus.Abandoned);

    private async Task<ActionResult<TourExecution>> Finish(string id, TourExecutionStatus status)
    {
        var execution = await _executions.GetByIdAsync(id);
        if (execution is null) return NotFound();
        if (execution.TouristId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
        if (execution.Status != TourExecutionStatus.Active) return BadRequest("Execution is not active.");

        execution.Status = status;
        execution.EndTime = DateTime.UtcNow;
        execution.LastActivity = execution.EndTime.Value;
        await _executions.ReplaceAsync(execution);
        return Ok(execution);
    }
}
