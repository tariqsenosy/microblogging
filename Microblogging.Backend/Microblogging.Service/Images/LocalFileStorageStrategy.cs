using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microblogging.Service.Images
{
    public class LocalFileStorageStrategy : IImageStorageStrategy
    {
        private readonly string _uploadPath;

        public LocalFileStorageStrategy(IWebHostEnvironment env)
        {
            _uploadPath = Path.Combine(env.ContentRootPath, "Uploads");
            Directory.CreateDirectory(_uploadPath);
        }

        public async Task<string> UploadAsync(Stream stream, string fileName)
        {
            var path = Path.Combine(_uploadPath, fileName);
            using var fileStream = new FileStream(path, FileMode.Create);
            await stream.CopyToAsync(fileStream);
            return $"/uploads/{fileName}";
        }

        public Task DeleteAsync(string fileName)
        {
            var path = Path.Combine(_uploadPath, fileName);
            if (File.Exists(path))
                File.Delete(path);
            return Task.CompletedTask;
        }
    }

}
