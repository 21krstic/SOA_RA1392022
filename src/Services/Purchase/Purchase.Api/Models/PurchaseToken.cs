using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Purchase.Api.Models;

public class PurchaseToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string TouristId { get; set; } = string.Empty;
    public string TourId { get; set; } = string.Empty;
    public string TourName { get; set; } = string.Empty;
    public decimal PricePaid { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
}
