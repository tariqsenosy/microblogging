using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microblogging.Service.Images
{
    public interface IImageStorageStrategy
    {
        Task<string> UploadAsync(Stream stream, string fileName);
        Task DeleteAsync(string fileName);
    }
}
