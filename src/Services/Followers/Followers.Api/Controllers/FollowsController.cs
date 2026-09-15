using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Followers.Api.Data;

namespace Followers.Api.Controllers;

public record FollowRequest(string FolloweeId);

[ApiController]
[Route("api/follows")]
public class FollowsController : ControllerBase
{
    private readonly FollowsRepository _repository;

    public FollowsController(FollowsRepository repository)
    {
        _repository = repository;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Follow(FollowRequest request)
    {
        var followerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (followerId == request.FolloweeId)
            return BadRequest("A user cannot follow themselves.");

        await _repository.FollowAsync(followerId, request.FolloweeId);
        return NoContent();
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Unfollow([FromQuery] string followeeId)
    {
        var followerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _repository.UnfollowAsync(followerId, followeeId);
        return NoContent();
    }

    [HttpGet("{userId}/is-following/{targetId}")]
    public async Task<ActionResult<bool>> IsFollowing(string userId, string targetId)
    {
        return Ok(await _repository.IsFollowingAsync(userId, targetId));
    }

    [HttpGet("{userId}/following")]
    public async Task<ActionResult<List<string>>> GetFollowing(string userId)
    {
        return Ok(await _repository.GetFollowingAsync(userId));
    }

    // Recommendations are derived from the caller's own follow graph, so —
    // unlike is-following/following, which describe an inherently public
    // social graph — this one is scoped to the caller.
    [Authorize]
    [HttpGet("{userId}/recommendations")]
    public async Task<ActionResult<List<string>>> GetRecommendations(string userId)
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) != userId) return Forbid();
        return Ok(await _repository.GetRecommendationsAsync(userId));
    }
}
