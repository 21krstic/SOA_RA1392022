using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Purchase.Api.Models;

namespace Purchase.Api.Data;

public class CartsRepository
{
    private readonly IMongoCollection<ShoppingCart> _carts;

    public CartsRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _carts = database.GetCollection<ShoppingCart>("carts");
    }

    public async Task<ShoppingCart> GetOrCreateAsync(string touristId)
    {
        var cart = await _carts.Find(c => c.TouristId == touristId).FirstOrDefaultAsync();
        if (cart is not null) return cart;

        cart = new ShoppingCart { TouristId = touristId };
        await _carts.InsertOneAsync(cart);
        return cart;
    }

    public Task ReplaceAsync(ShoppingCart cart) =>
        _carts.ReplaceOneAsync(c => c.TouristId == cart.TouristId, cart, new ReplaceOptions { IsUpsert = true });

    public Task ClearAsync(string touristId) =>
        _carts.UpdateOneAsync(c => c.TouristId == touristId, Builders<ShoppingCart>.Update.Set(c => c.Items, new List<CartItem>()));
}
