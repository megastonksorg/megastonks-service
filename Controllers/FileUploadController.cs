using Microsoft.AspNetCore.Mvc;
using Megastonks.Services;

namespace Megastonks.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class FileUploadController : BaseController
    {
        private readonly IFileUploadService _fileUploadService;

        public FileUploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPut("image")]
        public ActionResult<Uri> Image()
        {
            IFormFile file = Request.Form.Files.FirstOrDefault();
            var response = _fileUploadService.UploadImage(file);
            return Ok(response);
        }
    }
}

