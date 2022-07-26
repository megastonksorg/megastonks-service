using Microsoft.Extensions.Options;
using Azure.Storage.Blobs;
using Megastonks.Helpers;

namespace Megastonks.Services
{
    public class FileStorageService
    {
        private readonly IConfiguration Configuration;

        public FileStorageService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Uri UploadImage(byte [] imageFile)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(Configuration.GetConnectionString("AzureBlobStorage"));
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("images");

            string fileName = $"{Guid.NewGuid()}.png";
            BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
            using var memoryStream = new MemoryStream(imageFile, false);
            if (memoryStream != null) {
                blobClient.Upload(memoryStream);
            }

            return blobClient.Uri;
        }
    }
}