using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Purchase.Api.Models;

namespace Purchase.Api.Data;

public class PurchaseTokensRepository
{
    private readonly IMongoCollection<PurchaseToken> _tokens;

    public PurchaseTokensRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _tokens = database.GetCollection<PurchaseToken>("purchase_tokens");
    }

    public Task InsertManyAsync(IEnumerable<PurchaseToken> tokens) => _tokens.InsertManyAsync(tokens);

    public Task<List<PurchaseToken>> GetByTouristAsync(string touristId) =>
        _tokens.Find(t => t.TouristId == touristId).ToListAsync();

    public async Task<bool> IsPurchasedAsync(string touristId, string tourId) =>
        await _tokens.Find(t => t.TouristId == touristId && t.TourId == tourId).AnyAsync();
}
