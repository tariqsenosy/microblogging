using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace Microblogging.Service.Images
{
    

    public class AzureBlobStorageStrategy : IImageStorageStrategy
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobStorageStrategy(IConfiguration config)
        {
            var connectionString = config["AzureBlob:ConnectionString"];
            var containerName = config["AzureBlob:Container"];
            _containerClient = new BlobContainerClient(connectionString, containerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadAsync(Stream stream, string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(stream, overwrite: true);
            return blobClient.Uri.ToString();
        }

        public async Task DeleteAsync(string fileName)
        {
            await _containerClient.DeleteBlobIfExistsAsync(fileName);
        }
    }

}
