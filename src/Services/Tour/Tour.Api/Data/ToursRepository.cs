using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TourService.Models;

namespace TourService.Data;

public class ToursRepository
{
    private readonly IMongoCollection<Tour> _tours;

    public ToursRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _tours = database.GetCollection<Tour>("tours");
    }

    public Task CreateAsync(Tour tour) => _tours.InsertOneAsync(tour);

    public Task<Tour?> GetByIdAsync(string id) => _tours.Find(t => t.Id == id).FirstOrDefaultAsync()!;

    public Task<List<Tour>> GetByAuthorAsync(string authorId) =>
        _tours.Find(t => t.AuthorId == authorId).ToListAsync();

    public Task UpdateStatusAsync(string id, TourStatus status) =>
        _tours.UpdateOneAsync(t => t.Id == id, Builders<Tour>.Update.Set(t => t.Status, status));
}
