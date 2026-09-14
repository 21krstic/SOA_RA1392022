using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TourService.Models;

public enum KeyPointType { Start, End }

public class KeyPoint
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string TourId { get; set; } = string.Empty;
    public KeyPointType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
