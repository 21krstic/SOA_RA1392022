using MongoDB.Bson.Serialization.Attributes;

namespace TourService.Models;

public class TouristPosition
{
    [BsonId]
    public string TouristId { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
