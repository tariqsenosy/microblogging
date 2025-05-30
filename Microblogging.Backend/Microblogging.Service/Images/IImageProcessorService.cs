using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microblogging.Service.Images
{
    public interface IImageProcessorService
    {
        bool ValidateImage(IFormFile file);
        Dictionary<string, string> QueueImageForMultiSizeProcessing(IFormFile file);
        void Start(); 
    }
}
