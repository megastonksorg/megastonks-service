using Microsoft.AspNetCore.Mvc;
using Megastonks.Services;

namespace Megastonks.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class MediaUploadController : BaseController
    {
        private readonly IMediaUploadService _mediaUploadService;

        public MediaUploadController(IMediaUploadService mediaUploadService)
        {
            _mediaUploadService = mediaUploadService;
        }

        [HttpPut("imageFile")]
        public ActionResult<Uri> Image()
        {
            IFormFile file = Request.Form.Files.FirstOrDefault();
            var response = _mediaUploadService.UploadImageFile(file);
            return Ok(response);
        }

        [HttpPut("image")]
        public ActionResult<Uri> Image([FromBody] byte[] imageData)
        {
            var response = _mediaUploadService.UploadImage(imageData);
            return Ok(response);
        }

        [HttpPut("videoFile")]
        [RequestSizeLimit(bytes: 150_000_000)] //150MB
        public ActionResult<Uri> Video()
        {
            IFormFile file = Request.Form.Files.FirstOrDefault();
            var response = _mediaUploadService.UploadVideoFile(file);
            return Ok(response);
        }

        [HttpPut("video")]
        [RequestSizeLimit(bytes: 150_000_000)] //150MB
        public ActionResult<Uri> Video([FromBody] byte[] videoData)
        {
            var response = _mediaUploadService.UploadVideo(videoData);
            return Ok(response);
        }
    }
}