using Microsoft.AspNetCore.Mvc;
using Followers.Api.Data;

namespace Followers.Api.Controllers;

public record FollowRequest(string FollowerId, string FolloweeId);

[ApiController]
[Route("api/follows")]
public class FollowsController : ControllerBase
{
    private readonly FollowsRepository _repository;

    public FollowsController(FollowsRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> Follow(FollowRequest request)
    {
        if (request.FollowerId == request.FolloweeId)
            return BadRequest("A user cannot follow themselves.");

        await _repository.FollowAsync(request.FollowerId, request.FolloweeId);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Unfollow([FromQuery] string followerId, [FromQuery] string followeeId)
    {
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

    [HttpGet("{userId}/recommendations")]
    public async Task<ActionResult<List<string>>> GetRecommendations(string userId)
    {
        return Ok(await _repository.GetRecommendationsAsync(userId));
    }
}
