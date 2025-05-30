using Microblogging.Domain.Entities;
using System.Collections.Generic;

namespace Microblogging.Service.Services.Posts.DTO;

public class GetPostResponse
{
    public string Text { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string>? ImageVariants { get; set; }
    public string? OriginalImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public GetPostResponse(Post post)
    {
        Text = post.Text;
        Username = post.Username;
        CreatedAt = post.CreatedAt;
        OriginalImageUrl = post.OriginalImageUrl;
        Latitude = post.Latitude;
        Longitude = post.Longitude;
        ImageVariants=post.ImageVariants;
        
    }
}
