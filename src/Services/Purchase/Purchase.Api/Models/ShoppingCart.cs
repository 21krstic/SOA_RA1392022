using MongoDB.Bson.Serialization.Attributes;

namespace Purchase.Api.Models;

public class ShoppingCart
{
    [BsonId]
    public string TouristId { get; set; } = string.Empty;

    public List<CartItem> Items { get; set; } = [];

    [BsonIgnore]
    public decimal TotalPrice => Items.Sum(i => i.Price);
}

public class CartItem
{
    public string TourId { get; set; } = string.Empty;
    public string TourName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
