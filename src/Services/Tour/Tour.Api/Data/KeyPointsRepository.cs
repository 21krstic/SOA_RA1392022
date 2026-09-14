using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TourService.Models;

namespace TourService.Data;

public class KeyPointsRepository
{
    private readonly IMongoCollection<KeyPoint> _keyPoints;

    public KeyPointsRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _keyPoints = database.GetCollection<KeyPoint>("key_points");
    }

    public Task CreateAsync(KeyPoint keyPoint) => _keyPoints.InsertOneAsync(keyPoint);

    public Task<List<KeyPoint>> GetByTourAsync(string tourId) =>
        _keyPoints.Find(k => k.TourId == tourId).ToListAsync();
}
