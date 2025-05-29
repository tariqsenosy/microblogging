using Microsoft.AspNetCore.Http;

namespace Microblogging.Service.Posts.DTO;

public class CreatePostRequest
{
    public string Text { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}
