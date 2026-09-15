using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stakeholders.Api.Data;
using Stakeholders.Api.Dtos;

namespace Stakeholders.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UsersRepository _users;

    public UsersController(UsersRepository users)
    {
        _users = users;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(string id)
    {
        var user = await _users.GetByIdAsync(id);
        if (user is null) return NotFound();

        return Ok(new UserProfileResponse(
            user.Id, user.Username, user.Role,
            user.Profile.FirstName, user.Profile.LastName,
            user.Profile.ProfileImagePath, user.Profile.Biography, user.Profile.Motto));
    }

    // Lets the frontend resolve a username to an id, e.g. to follow someone by name.
    [HttpGet("by-username/{username}")]
    public async Task<ActionResult<UserProfileResponse>> GetByUsername(string username)
    {
        var user = await _users.GetByUsernameAsync(username);
        if (user is null) return NotFound();

        return Ok(new UserProfileResponse(
            user.Id, user.Username, user.Role,
            user.Profile.FirstName, user.Profile.LastName,
            user.Profile.ProfileImagePath, user.Profile.Biography, user.Profile.Motto));
    }

    [Authorize]
    [HttpPut("{id}/profile")]
    public async Task<IActionResult> UpdateProfile(string id, UpdateProfileRequest request)
    {
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (callerId != id) return Forbid();

        var user = await _users.GetByIdAsync(id);
        if (user is null) return NotFound();

        user.Profile.FirstName = request.FirstName;
        user.Profile.LastName = request.LastName;
        user.Profile.ProfileImagePath = request.ProfileImagePath;
        user.Profile.Biography = request.Biography;
        user.Profile.Motto = request.Motto;

        await _users.UpdateProfileAsync(id, user.Profile);
        return NoContent();
    }
}
