using Microsoft.AspNetCore.Mvc;
using Stakeholders.Api.Data;
using Stakeholders.Api.Dtos;
using Stakeholders.Api.Models;
using Stakeholders.Api.Services;

namespace Stakeholders.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UsersRepository _users;
    private readonly JwtTokenService _tokenService;

    public AuthController(UsersRepository users, JwtTokenService tokenService)
    {
        _users = users;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (request.Role == Role.Administrator)
            return BadRequest("Administrator accounts cannot be self-registered.");

        if (await _users.UsernameOrEmailExistsAsync(request.Username, request.Email))
            return Conflict("Username or email already in use.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        await _users.CreateAsync(user);

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Id, user.Username, user.Role));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _users.GetByUsernameAsync(request.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password.");

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Id, user.Username, user.Role));
    }
}
