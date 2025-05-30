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
        private readonly IServiceScopeFactory _scopeFactory;

        public ImageStorageStrategyFactory(IServiceProvider provider, IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            _provider = provider;
            _config = config;
            _scopeFactory = scopeFactory;
        }

        public IImageStorageStrategy GetStrategy()
        {
            var provider = _config.GetValue<string>("Storage:Provider");

            using var scope = _scopeFactory.CreateScope();
            return provider.ToLower() switch
            {
                "local" => scope.ServiceProvider.GetRequiredService<LocalFileStorageStrategy>(),
                "azureblob" => scope.ServiceProvider.GetRequiredService<AzureBlobStorageStrategy>(),
                _ => throw new InvalidOperationException("Unsupported storage provider")
            };
        }
    }

}
