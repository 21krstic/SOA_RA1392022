using Microsoft.Extensions.Options;
using MongoDB.Bson;
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

    public async Task<Tour?> GetByIdAsync(string id) =>
        ObjectId.TryParse(id, out _) ? await _tours.Find(t => t.Id == id).FirstOrDefaultAsync() : null;

    public Task<List<Tour>> GetByAuthorAsync(string authorId) =>
        _tours.Find(t => t.AuthorId == authorId).ToListAsync();

    public Task UpdateStatusAsync(string id, TourStatus status) =>
        _tours.UpdateOneAsync(t => t.Id == id, Builders<Tour>.Update.Set(t => t.Status, status));

    public Task UpdatePriceAsync(string id, decimal price) =>
        _tours.UpdateOneAsync(t => t.Id == id, Builders<Tour>.Update.Set(t => t.Price, price));
}
