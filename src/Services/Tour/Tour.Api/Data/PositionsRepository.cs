using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TourService.Models;

namespace TourService.Data;

public class PositionsRepository
{
    private readonly IMongoCollection<TouristPosition> _positions;

    public PositionsRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _positions = database.GetCollection<TouristPosition>("tourist_positions");
    }

    public Task<TouristPosition?> GetAsync(string touristId) =>
        _positions.Find(p => p.TouristId == touristId).FirstOrDefaultAsync()!;

    public Task SetAsync(string touristId, double latitude, double longitude) =>
        _positions.ReplaceOneAsync(
            p => p.TouristId == touristId,
            new TouristPosition { TouristId = touristId, Latitude = latitude, Longitude = longitude, UpdatedAt = DateTime.UtcNow },
            new ReplaceOptions { IsUpsert = true });
}
