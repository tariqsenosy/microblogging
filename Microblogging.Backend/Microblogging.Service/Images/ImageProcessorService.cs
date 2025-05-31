using Microblogging.Service.Images;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Microblogging.Service.Images;

public class ImageProcessorService : IImageProcessorService
{
    private readonly ImageStorageStrategyFactory _storageFactory;
    private readonly IWebHostEnvironment _env;
    private static readonly ConcurrentQueue<(byte[] ImageData, string ImageId)> _queue = new();

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

  

    public async Task<string> UploadOriginalAndQueueSizesAsync(IFormFile file, string imageId)
    {
        var strategy = _storageFactory.GetStrategy();
        var originalPath = $"/uploads/{imageId}-original.webp";

        using var input = file.OpenReadStream();
        using var image = Image.Load(input);

        using var ms = new MemoryStream();
        image.Save(ms, new WebpEncoder());
        ms.Position = 0;
        strategy.UploadAsync(ms, $"{imageId}-original.webp").Wait(); // upload immediately

        // enqueue resized versions
        using var bufferStream = new MemoryStream();
        file.CopyTo(bufferStream);
        var buffer = bufferStream.ToArray();

        _queue.Enqueue((buffer, imageId)); 

        return originalPath;
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
                    var strategy = _storageFactory.GetStrategy();
                    using var stream = new MemoryStream(item.ImageData);
                    using var originalImage = await Image.LoadAsync(stream);
                     
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

    public Dictionary<string, string> GetPreviewUrls(string imageId)
    {
        return Sizes.ToDictionary(
            size => $"{size}w",
            size => $"/uploads/{imageId}-{size}w.webp"
        );
    }


}

