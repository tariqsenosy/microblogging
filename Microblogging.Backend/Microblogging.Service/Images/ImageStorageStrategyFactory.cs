using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microblogging.Service.Images
{
    public class ImageStorageStrategyFactory
    {
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _config;

        public ImageStorageStrategyFactory(IServiceProvider provider, IConfiguration config)
        {
            _provider = provider;
            _config = config;
        }

        public IImageStorageStrategy GetStrategy()
        {
            var provider = _config.GetValue<string>("Storage:Provider");

            return provider?.ToLower() switch
            {
               "local" => _provider.GetRequiredService<LocalFileStorageStrategy>(),
               "azureblob" => _provider.GetRequiredService<AzureBlobStorageStrategy>(),
               // add more providers
                _ => throw new InvalidOperationException("Unsupported storage provider")
            };
        }
    }

}
