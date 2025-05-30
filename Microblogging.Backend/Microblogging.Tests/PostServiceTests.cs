using AspNetCore.Identity.Mongo.Model;
using FluentAssertions;
using Microblogging.Domain.Entities;
using Microblogging.Repository;
using Microblogging.Service.Images;
using Microblogging.Service.Posts.DTO;
using Microblogging.Service.Services.Posts;
using Microblogging.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Net;

public class PostServiceTests
{
    private readonly Mock<IBaseRepository<Post>> _repoMock = new();
    private readonly Mock<UserManager<MongoUser>> _userManagerMock;
    private readonly Mock<IImageProcessorService> _imageProcessorMock = new();

    private readonly PostService _service;

    public PostServiceTests()
    {
        var store = new Mock<IUserStore<MongoUser>>();
        _userManagerMock = new Mock<UserManager<MongoUser>>(store.Object, null, null, null, null, null, null, null, null);

        _service = new PostService(_repoMock.Object, _userManagerMock.Object, _imageProcessorMock.Object);
    }

    [Fact]
    public async Task CreatePostAsync_ShouldReturnError_WhenTextIsTooLong()
    {
        var request = new CreatePostRequest { Text = new string('a', 141) };

        var result = await _service.CreatePostAsync(request, "test_user");

        result.IsSuccess.Should().BeFalse();
        result.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePostAsync_ShouldReturnError_WhenTextIsEmpty()
    {
        var request = new CreatePostRequest { Text = "" };

        var result = await _service.CreatePostAsync(request, "test_user");

        result.IsSuccess.Should().BeFalse();
        result.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePostAsync_ShouldReturnError_WhenImageIsInvalid()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(3 * 1024 * 1024); // invalid size
        mockFile.Setup(f => f.ContentType).Returns("image/gif");

        _imageProcessorMock.Setup(x => x.ValidateImage(It.IsAny<IFormFile>())).Returns(false);

        var request = new CreatePostRequest { Text = "Hello", Image = mockFile.Object };

        var result = await _service.CreatePostAsync(request, "test_user");

        result.IsSuccess.Should().BeFalse();
        result.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePostAsync_ShouldCreatePost_WithValidTextOnly()
    {
        var request = new CreatePostRequest { Text = "Hello World" };

        _repoMock.Setup(x => x.AddAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.SaveDbAsync()).ReturnsAsync(true);

        var result = await _service.CreatePostAsync(request, "test_user");

        result.IsSuccess.Should().BeTrue();
        result.HttpStatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePostAsync_ShouldCreatePost_WithImage()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");

        _imageProcessorMock.Setup(x => x.ValidateImage(It.IsAny<IFormFile>())).Returns(true);
        _imageProcessorMock.Setup(x => x.QueueImageForMultiSizeProcessing(It.IsAny<IFormFile>())).Returns(new Dictionary<string, string> { { "original", "/uploads/1.webp" } });

        var request = new CreatePostRequest { Text = "Hello", Image = mockFile.Object };

        _repoMock.Setup(x => x.AddAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.SaveDbAsync()).ReturnsAsync(true);

        var result = await _service.CreatePostAsync(request, "test_user");

        result.IsSuccess.Should().BeTrue();
        result.HttpStatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePostAsync_ShouldFail_WhenSaveDbFails()
    {
        var request = new CreatePostRequest { Text = "Test post" };

        _repoMock.Setup(x => x.AddAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.SaveDbAsync()).ReturnsAsync(false);

        var result = await _service.CreatePostAsync(request, "test_user");

        result.IsSuccess.Should().BeTrue();
        result.HttpStatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePostAsync_ShouldStoreCorrectUsername()
    {
        Post captured = null;

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Post>()))
                 .Callback<Post>(p => captured = p)
                 .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveDbAsync()).ReturnsAsync(true);

        var request = new CreatePostRequest { Text = "Hello" };
        await _service.CreatePostAsync(request, "abjjad");

        captured.Username.Should().Be("abjjad");
    }

    [Fact]
    public async Task CreatePostAsync_ShouldSetDefaultLocation()
    {
        Post captured = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Post>()))
                 .Callback<Post>(p => captured = p)
                 .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveDbAsync()).ReturnsAsync(true);

        var request = new CreatePostRequest { Text = "Hello" };
        await _service.CreatePostAsync(request, "abjjad");

        captured.Latitude.Should().BeInRange(-90, 90);
        captured.Longitude.Should().BeInRange(-180, 180);
    }

    [Fact]
    public async Task CreatePostAsync_ShouldSetImageVariants()
    {
        var variants = new Dictionary<string, string> { { "original", "/uploads/original.webp" }, { "400w", "/uploads/400.webp" } };

        _imageProcessorMock.Setup(x => x.ValidateImage(It.IsAny<IFormFile>())).Returns(true);
        _imageProcessorMock.Setup(x => x.QueueImageForMultiSizeProcessing(It.IsAny<IFormFile>())).Returns(variants);

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/webp");

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveDbAsync()).ReturnsAsync(true);

        var request = new CreatePostRequest { Text = "Hi", Image = mockFile.Object };
        var result = await _service.CreatePostAsync(request, "abjjad");

        result.Data.OriginalImageUrl.Should().Be("/uploads/original.webp");
    }

    [Fact]
    public async Task CreatePostAsync_ShouldFail_IfExceptionThrown()
    {
        var request = new CreatePostRequest { Text = "Crash" };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Post>())).ThrowsAsync(new Exception("fail"));

        Func<Task> act = async () => await _service.CreatePostAsync(request, "user");

        await act.Should().ThrowAsync<Exception>().WithMessage("fail");
    }
}
