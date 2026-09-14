using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TourService.Models;

public enum TourExecutionStatus { Active, Completed, Abandoned }

public class TourExecution
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string TouristId { get; set; } = string.Empty;
    public string TourId { get; set; } = string.Empty;
    public TourExecutionStatus Status { get; set; } = TourExecutionStatus.Active;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public List<CompletedKeyPoint> CompletedKeyPoints { get; set; } = [];
}

public class CompletedKeyPoint
{
    public string KeyPointId { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
