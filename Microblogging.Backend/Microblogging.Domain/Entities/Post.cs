using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Microblogging.Domain.Entities;

public class Post
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? OriginalImageUrl { get; set; }
    public Dictionary<string, string>? ImageVariants { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Username { get; set; } = string.Empty;
}
