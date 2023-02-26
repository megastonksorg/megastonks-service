﻿using Microsoft.AspNetCore.Mvc;
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
    }
}