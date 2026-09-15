using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TourService.Models;

public enum TourDifficulty { Easy, Medium, Hard }

public enum TourStatus { Draft, Published, Archived }

public static class TourStatusTransitions
{
    public static bool IsValid(TourStatus from, TourStatus to) => (from, to) is
        (TourStatus.Draft, TourStatus.Published) or
        (TourStatus.Published, TourStatus.Archived);
}

public class Tour
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string AuthorId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TourDifficulty Difficulty { get; set; }
    public List<string> Tags { get; set; } = [];
    public TourStatus Status { get; set; } = TourStatus.Draft;
    public decimal Price { get; set; } = 0;
    public double LengthKm { get; set; }
    public int DurationMinutes { get; set; }
}
