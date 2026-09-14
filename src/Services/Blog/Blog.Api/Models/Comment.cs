using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogService.Models;

public class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string BlogId { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
