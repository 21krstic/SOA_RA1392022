using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TourService.Models;

namespace TourService.Data;

public class ReviewsRepository
{
    private readonly IMongoCollection<Review> _reviews;

    public ReviewsRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _reviews = database.GetCollection<Review>("reviews");
    }

    public Task CreateAsync(Review review) => _reviews.InsertOneAsync(review);

    public Task<List<Review>> GetByTourAsync(string tourId) =>
        _reviews.Find(r => r.TourId == tourId).ToListAsync();
}
