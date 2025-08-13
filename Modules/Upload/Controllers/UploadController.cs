using course_service.Attributes;
using course_service.Modules.Upload.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace course_service.Modules.Upload.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IUploadService _uploadService;

        public UploadController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [RequireAdmin]
        [HttpGet("signature")]
        public async Task<IActionResult> GetSignature()
        {
            var result = await _uploadService.GenerateSignatureAsync();
            return Ok(result);
        }
    }
}
