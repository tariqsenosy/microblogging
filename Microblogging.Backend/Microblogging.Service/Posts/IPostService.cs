using Microblogging.Service.Posts.DTO;
using Microblogging.Service.Services.Posts.DTO;
using Microblogging.Shared;

namespace Microblogging.Service.Posts;

public interface IPostService
{
    Task<FuncResponseWithValue<GetPostResponse>> CreatePostAsync(CreatePostRequest request, string username);
    Task<PaginatedOutPut<GetPostResponse>> GetTimelineAsync(BasePage basePage, string? orderBy = "CreatedAt");
}
