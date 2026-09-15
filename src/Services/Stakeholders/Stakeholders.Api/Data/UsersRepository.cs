using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Stakeholders.Api.Models;

namespace Stakeholders.Api.Data;

public class UsersRepository
{
    private readonly IMongoCollection<User> _users;

    public UsersRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _users = database.GetCollection<User>("users");
    }

    public async Task<User?> GetByIdAsync(string id) =>
        ObjectId.TryParse(id, out _) ? await _users.Find(u => u.Id == id).FirstOrDefaultAsync() : null;

    public Task<User?> GetByUsernameAsync(string username) =>
        _users.Find(u => u.Username == username).FirstOrDefaultAsync()!;

    public async Task<bool> UsernameOrEmailExistsAsync(string username, string email) =>
        await _users.Find(u => u.Username == username || u.Email == email).AnyAsync();

    public Task CreateAsync(User user) => _users.InsertOneAsync(user);

    public Task UpdateProfileAsync(string id, UserProfile profile) =>
        _users.UpdateOneAsync(u => u.Id == id, Builders<User>.Update.Set(u => u.Profile, profile));
}
