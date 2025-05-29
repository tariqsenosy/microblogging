using Microblogging.Domain.Entities;

namespace Microblogging.Service.Services.Posts.DTO;
public class GetPostResponse
{
    public string Text { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public GetPostResponse(Post post) // Fully qualify the Post type to avoid ambiguity
    {
        Text = post.Text;
        Username = post.Username;
        CreatedAt = post.CreatedAt;
        ImageUrl = post.ProcessedImageUrl ?? post.OriginalImageUrl;
        Latitude = post.Latitude;
        Longitude = post.Longitude;
    }
}
