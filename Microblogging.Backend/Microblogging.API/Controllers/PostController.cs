using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microblogging.Service.Posts;
using Microblogging.Service.Posts.DTO;
using Microblogging.Shared;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;

    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreatePostRequest request)
    {
        var username = User.FindFirstValue(ClaimTypes.Name);
        var result = await _postService.CreatePostAsync(request, username!);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet("timeline")]
    public async Task<IActionResult> GetTimeline([FromQuery] BasePage basePage, [FromQuery] string? orderBy = "CreatedAt")
    {
        var result = await _postService.GetTimelineAsync(basePage, orderBy);
        return StatusCode((int)result.StatusCode, result);
    }
}
