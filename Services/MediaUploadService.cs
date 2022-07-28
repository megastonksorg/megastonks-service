using Azure.Storage.Blobs;
using Megastonks.Helpers;

namespace Megastonks.Services
{
    public interface IMediaUploadService
    {
        Uri UploadImage(IFormFile file);
        Uri UploadImage(byte[] imageData);
    }

    public class MediaUploadService : IMediaUploadService
    {
        private readonly IConfiguration Configuration;

        public MediaUploadService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Uri UploadImage(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    throw new AppException(message: "No File In Request: Please attach a file to the request");
                }

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
            catch (Exception e)
            {
                throw new AppException(e.Message);
            }
        }

        public Uri UploadImage(byte[] imageData)
        {
            try
            {
                if (imageData == null)
                {
                    throw new AppException(message: "No Data In Request: Please attach an image data to the request");
                }

                if (imageData.Length > 2000000)
                {
                    throw new AppException(message: "Image Size too large: Please upload an image that is less than 2MB");
                }

                Stream stream = new MemoryStream(imageData);
                const string allowedFileExtension = ".png";

                BlobServiceClient blobServiceClient = new(Configuration.GetConnectionString("AzureBlobStorage"));
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("images");

                string fileName = $"{Guid.NewGuid()}{allowedFileExtension}";
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
                blobClient.Upload(stream);
                return blobClient.Uri;
            }
            catch (Exception e)
            {
                throw new AppException(e.Message);
            }
        }
    }
}