using MARecognition.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MARecognition.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly FrameExtractorService _extractor;

        public VideoController(FrameExtractorService extractor)
        {
            _extractor = extractor;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadPath = Path.Combine("videos", file.FileName);
            Directory.CreateDirectory("videos");

            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Estrazione frame
            string framesFolder = Path.Combine("data", "frames");
            int frameCount = _extractor.ExtractFrames(uploadPath, framesFolder, fpsToExtract: 1);

            return Ok(new { message = "Video processed", totalFrames = frameCount });
        }
    }

}
