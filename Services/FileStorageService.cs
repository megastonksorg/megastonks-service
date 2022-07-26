﻿using Microsoft.Extensions.Options;
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

        public Uri UploadImage(IFormFile file)
        {
            const string allowedFileExtension = ".png";
            string fileExtension = Path.GetExtension(file.FileName);
            if (file.Length > 2000000)
            {
                throw new AppException(message: "File Size too large: Please upload a file that is less than 2MB");
            }
            else if (allowedFileExtension != fileExtension)
            {
                throw new AppException(message: "Invalid File Format: Please upload a png file");
            }
            else
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(Configuration.GetConnectionString("AzureBlobStorage"));
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("images");

                string fileName = $"{Guid.NewGuid()}{allowedFileExtension}";
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
                blobClient.Upload(file.OpenReadStream());
                return blobClient.Uri;
            }
        }
    }
}