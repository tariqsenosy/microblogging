using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
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
            var rootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _uploadPath = Path.Combine(rootPath, "uploads");

            if (!Directory.Exists(_uploadPath))
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
