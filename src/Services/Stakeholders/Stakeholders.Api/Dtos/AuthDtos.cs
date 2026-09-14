using Stakeholders.Api.Models;

namespace Stakeholders.Api.Dtos;

public record RegisterRequest(string Username, string Email, string Password, Role Role);

public record LoginRequest(string Username, string Password);

public record AuthResponse(string Token, string UserId, string Username, Role Role);

public record UpdateProfileRequest(string FirstName, string LastName, string? ProfileImagePath, string? Biography, string? Motto);

public record UserProfileResponse(string Id, string Username, Role Role, string FirstName, string LastName, string? ProfileImagePath, string? Biography, string? Motto);
