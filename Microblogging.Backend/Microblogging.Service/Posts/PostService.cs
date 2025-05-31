using AspNetCore.Identity.Mongo.Model;
using Microblogging.Domain.Entities;
using Microblogging.Repository;
using Microblogging.Service.Images;
using Microblogging.Service.Posts;
using Microblogging.Service.Posts.DTO;
using Microblogging.Service.Services.Posts.DTO;
using Microblogging.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using SixLabors.ImageSharp.Processing.Processors;
using System.Net;

namespace Microblogging.Service.Services.Posts;

public class PostService : IPostService
{
    private readonly IBaseRepository<Post> _postRepository;
    private readonly UserManager<MongoUser> _userManager;
    private readonly IImageProcessorService _imageProcessor;

    public PostService(IBaseRepository<Post> postRepository,
                       UserManager<MongoUser> userManager,
                       IImageProcessorService imageProcessor)
    {
        _postRepository = postRepository;
        _userManager = userManager;
        _imageProcessor = imageProcessor;

        _imageProcessor.Start();
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

        Dictionary<string, string>? imageVariants = null;
        string? originalImageUrl = null;

        if (request.Image != null)
        {
            if (!_imageProcessor.ValidateImage(request.Image))
            {
                return new FuncResponseWithValue<GetPostResponse>(
                    null,
                    HttpStatusCode.BadRequest,
                    ResponseCode.Error,
                    "Only JPG, PNG, or WebP images are allowed and must be ≤ 2MB."
                );
            }

            var imageId = Guid.NewGuid().ToString();
            originalImageUrl = await _imageProcessor.UploadOriginalAndQueueSizesAsync(request.Image, imageId);
            imageVariants = _imageProcessor.GetPreviewUrls(imageId);
        }

        var post = new Post
        {
            Text = request.Text,
            Username = username,
            ImageVariants = imageVariants,
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
            var filter = Builders<Post>.Filter.Empty;

            var skip = ((basePage.Page ?? 1) - 1) * (basePage.PageSize ?? 10);
            var limit = basePage.PageSize ?? 10;

            var sort = orderBy?.ToLower() switch
            {
                "createdat" => Builders<Post>.Sort.Descending(p => p.CreatedAt),
                _ => Builders<Post>.Sort.Descending(p => p.CreatedAt)
            };

            var collection = _postRepository.Collection;
            var totalCount = await collection.CountDocumentsAsync(filter);
            var posts = await collection.Find(filter)
                                        .Sort(sort)
                                        .Skip(skip)
                                        .Limit(limit)
                                        .ToListAsync();

            var result = posts.Select(post => new GetPostResponse(post)).ToList();

            return new PaginatedOutPut<GetPostResponse>(
                result,
                (int)totalCount,
                basePage.Page,
                basePage.PageSize,
                ResponseCode.Success,
                HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in GetTimelineAsync: " + ex.Message);

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

    private double GetRandomLatitude() =>
        new Random().NextDouble() * 180 - 90;

    private double GetRandomLongitude() =>
        new Random().NextDouble() * 360 - 180;


}
