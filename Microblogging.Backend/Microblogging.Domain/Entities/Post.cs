namespace Microblogging.Domain.Entities;

public class Post
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? OriginalImageUrl { get; set; }
    public string? ProcessedImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Username { get; set; } = string.Empty;
}
