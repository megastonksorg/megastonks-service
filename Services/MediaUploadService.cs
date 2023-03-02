using Azure.Storage.Blobs;
using Megastonks.Helpers;

namespace Megastonks.Services
{
    public interface IMediaUploadService
    {
        Uri UploadImageFile(IFormFile file);
        Uri UploadImage(byte[] imageData);
        Uri UploadVideoFile(IFormFile file);
        Uri UploadVideo(byte[] videoData);
    }

    public class MediaUploadService : IMediaUploadService
    {
        private readonly int imageSizeLimit = 10_000_000; //10MB in bytes
        private readonly int videoSizeLimit = 60_000_000; //60MB in bytes
        private readonly string imageSizeLimitDescription = "2MB"; //2MB
        private readonly string videoSizeLimitDescription = "60MB"; //60MB
        private readonly string imageFileExtension = ".png";
        private readonly string videoFileExtension = ".mp4";

        private readonly string connectionStringSection = "AzureBlobStorage";
        private readonly string imageContainer = "images";
        private readonly string videoContainer = "videos";

        private readonly ILogger<MediaUploadService> _logger;
        private readonly IConfiguration _configuration;

        public MediaUploadService(ILogger<MediaUploadService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Uri UploadImageFile(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    throw new AppException(message: "No File In Request: Please attach a file to the request");
                }

                string fileExtension = Path.GetExtension(file.FileName);
                if (file.Length > imageSizeLimit)
                {
                    throw new AppException(message: $"File Size too large: Please upload a file that is less than {imageSizeLimitDescription}");
                }
                else if (imageFileExtension != fileExtension)
                {
                    throw new AppException(message: $"Invalid File Format: Please upload a {imageFileExtension} file");
                }
                else
                {
                    BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString(connectionStringSection));
                    BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(imageContainer);

                    string fileName = $"{Guid.NewGuid()}{imageFileExtension}";
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

                Stream stream = new MemoryStream(imageData);

                if (stream.Length > imageSizeLimit)
                {
                    throw new AppException(message: $"Image Size too large: Please upload an image that is less than {imageSizeLimitDescription}");
                }

                BlobServiceClient blobServiceClient = new(_configuration.GetConnectionString(connectionStringSection));
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(imageContainer);

                string fileName = $"{Guid.NewGuid()}{imageFileExtension}";
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
                blobClient.Upload(stream);
                return blobClient.Uri;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new AppException(e.Message);
            }
        }

        public Uri UploadVideoFile(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    throw new AppException(message: "No File In Request: Please attach a file to the request");
                }

                string fileExtension = Path.GetExtension(file.FileName);
                if (file.Length > videoSizeLimit)
                {
                    throw new AppException(message: $"File Size too large: Please upload a file that is less than {videoSizeLimitDescription}");
                }
                else if (videoFileExtension != fileExtension)
                {
                    throw new AppException(message: $"Invalid File Format: Please upload an {videoFileExtension} file");
                }
                else
                {
                    BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString(connectionStringSection));
                    BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(videoContainer);

                    string fileName = $"{Guid.NewGuid()}{videoFileExtension}";
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

        public Uri UploadVideo(byte[] videoData)
        {
            try
            {
                if (videoData == null)
                {
                    throw new AppException(message: "No Data In Request: Please attach video data to the request");
                }

                Stream stream = new MemoryStream(videoData);

                if (stream.Length > videoSizeLimit)
                {
                    throw new AppException(message: $"Video Size too large: Please upload a video that is less than {videoSizeLimitDescription}");
                }

                BlobServiceClient blobServiceClient = new(_configuration.GetConnectionString(connectionStringSection));
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(videoContainer);

                string fileName = $"{Guid.NewGuid()}{videoFileExtension}";
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
                blobClient.Upload(stream);
                return blobClient.Uri;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new AppException(e.Message);
            }
        }
    }
}