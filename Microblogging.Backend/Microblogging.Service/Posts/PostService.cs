
using Microblogging.Domain.Entities;
using Microblogging.Repository;
using Microblogging.Service.Posts;
using Microblogging.Service.Posts.DTO;
using Microblogging.Service.Services.Posts.DTO;
using Microblogging.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;


namespace Microblogging.Service.Services.Posts;

public class PostService : IPostService
{
    private readonly IBaseRepository<Post> _postRepository;
    private readonly UserManager<User> _userManager;

    public PostService(IBaseRepository<Post> postRepository, UserManager<User> userManager)
    {
        _postRepository = postRepository;
        _userManager = userManager;
    }

    public async Task<FuncResponseWithValue<GetPostResponse>> CreatePostAsync(CreatePostRequest request, string username)
    {
        if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length > 140)
        {
            return new FuncResponseWithValue<GetPostResponse>(
                null,
                 HttpStatusCode.BadRequest,
                ResponseCode.Error,
                "Text is required and must be 140 characters or less."
            );
        }

        string? originalImageUrl = null;

        if (request.Image != null)
        {
            if (!ValidateImage(request.Image))
            {
                return new FuncResponseWithValue<GetPostResponse>(
                    null,
                     HttpStatusCode.BadRequest,
                    ResponseCode.Error,
                    "Only JPG, PNG, or WebP images are allowed and must be ≤ 2MB."
                );
            }

            // In a real app this would be uploaded and return a real URL
            originalImageUrl = $"https://mockstorage.com/uploads/{Guid.NewGuid()}.webp";
        }

        var post = new Post
        {
            Text = request.Text,
            Username = username,
            OriginalImageUrl = originalImageUrl,
            Latitude = GetRandomLatitude(),
            Longitude = GetRandomLongitude()
        };

        await _postRepository.AddAsync(post);
        await _postRepository.SaveDbAsync();

        return new FuncResponseWithValue<GetPostResponse>(
            new GetPostResponse(post),
             HttpStatusCode.Created,
            ResponseCode.Success
        );
    }

    public async Task<PaginatedOutPut<GetPostResponse>> GetTimelineAsync(BasePage basePage, string? orderBy = "CreatedAt")
    {
        try
        {
            var query = _postRepository.Search(x => true);

            // Ordering logic (can extend for other fields later)
            if (orderBy?.ToLower() == "createdat")
                query = query.OrderByDescending(x => x.CreatedAt);

            var totalCount = await query.CountAsync();

            if (basePage.Page is not null && basePage.PageSize is not null)
            {
                int skip = (basePage.Page.Value - 1) * basePage.PageSize.Value;
                query = query.Skip(skip).Take(basePage.PageSize.Value);
            }

            var data = await query.ToListAsync();
            var result = data.Select(post => new GetPostResponse(post)).ToList();

            return new PaginatedOutPut<GetPostResponse>(
                result,
                totalCount,
                basePage.Page,
                basePage.PageSize,
                ResponseCode.Success,
                HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            return new PaginatedOutPut<GetPostResponse>(
                null,
                0,
                basePage.Page,
                basePage.PageSize,
                ResponseCode.Error,
                HttpStatusCode.InternalServerError
            ); 
        }
    }

    private bool ValidateImage(IFormFile file)
    {
        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        return file.Length <= 2 * 1024 * 1024 && allowedContentTypes.Contains(file.ContentType.ToLower());
    }

    private double GetRandomLatitude() =>
        new Random().NextDouble() * 180 - 90;

    private double GetRandomLongitude() =>
        new Random().NextDouble() * 360 - 180;
}
