using Microblogging.Service.Images;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using System.Collections.Concurrent;

namespace Microblogging.Service.Images;

public class ImageProcessorService : IImageProcessorService
{
    private readonly ImageStorageStrategyFactory _storageFactory;
    private readonly IWebHostEnvironment _env;
    private static readonly ConcurrentQueue<(Stream ImageStream, string ImageId)> _queue = new();
    private static readonly HashSet<string> _processing = new();
    private static readonly int[] Sizes = new[] { 400, 800, 1200 };

    public ImageProcessorService(ImageStorageStrategyFactory storageFactory, IWebHostEnvironment env)
    {
        _storageFactory = storageFactory;
        _env = env;
    }

    public bool ValidateImage(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        return file.Length <= 2 * 1024 * 1024 && allowedTypes.Contains(file.ContentType.ToLower());
    }

    public Dictionary<string, string> QueueImageForMultiSizeProcessing(IFormFile file)
    {
        var imageId = Guid.NewGuid().ToString();

        using var tempStream = new MemoryStream();
        file.CopyTo(tempStream);
        var buffer = tempStream.ToArray();

        var memoryStream = new MemoryStream(buffer);
        _queue.Enqueue((memoryStream, imageId));

        var result = new Dictionary<string, string>
        {
            ["original"] = $"/uploads/{imageId}-original.webp"
        };

        foreach (var size in Sizes)
        {
            result[$"{size}w"] = $"/uploads/{imageId}-{size}w.webp";
        }

        return result;
    }


    public void Start()
    {
        Task.Run(ProcessLoop);
    }

    private async Task ProcessLoop()
    {
        while (true)
        {
            if (_queue.TryDequeue(out var item))
            {
                if (_processing.Contains(item.ImageId)) continue;
                _processing.Add(item.ImageId);

                try
                {
                    
                    using var originalImage = await Image.LoadAsync(item.ImageStream);
                    var strategy = _storageFactory.GetStrategy();
                    //save original first 
                    using (var msOriginal = new MemoryStream())
                    {
                        await originalImage.SaveAsync(msOriginal, new WebpEncoder());
                        msOriginal.Position = 0;
                        await strategy.UploadAsync(msOriginal, $"{item.ImageId}-original.webp");
                    }

                    // common sizes 
                    foreach (var size in Sizes)
                    {
                        var resized = originalImage.Clone(ctx => ctx.Resize(new ResizeOptions
                        {
                            Size = new Size(size, 0),
                            Mode = ResizeMode.Max
                        }));

                        using var ms = new MemoryStream();
                        await resized.SaveAsync(ms, new WebpEncoder());
                        ms.Position = 0;

                        var fileName = $"{item.ImageId}-{size}w.webp";
                        await strategy.UploadAsync(ms, fileName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Image processing failed for {item.ImageId}: {ex.Message}");
                }
                finally
                {
                    _processing.Remove(item.ImageId);
                }
            }

            await Task.Delay(500);
        }
    }
}
