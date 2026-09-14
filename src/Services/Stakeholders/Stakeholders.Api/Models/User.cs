using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Stakeholders.Api.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }
    public UserProfile Profile { get; set; } = new();
}

public class UserProfile
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }
    public string? Biography { get; set; }
    public string? Motto { get; set; }
}
