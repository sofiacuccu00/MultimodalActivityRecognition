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

            // Extraction frames
            string framesFolder = Path.Combine("data", "frames");
            int frameCount = _extractor.ExtractFrames(uploadPath, framesFolder, fpsToExtract: 1);

            return Ok(new { message = "Video processed", totalFrames = frameCount });
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeVideo([FromQuery] string modelName = "llava:latest")
        {
            string framesFolder = Path.Combine("data", "frames");

            if (!Directory.Exists(framesFolder))
                return BadRequest("No frames found. Upload and extract frames first.");

            var frameFiles = Directory.GetFiles(framesFolder, "*.jpg");
            if (frameFiles.Length < 3)
                return BadRequest("Not enough frames to analyze (minimum 3 required).");

            // VideoAnalyzerService 
            var analyzer = new VideoAnalyzerService(modelName);
            var eventIntervals = await analyzer.RecognizeVideoActions(framesFolder, frameFiles.Length);

            // Return merged ranges 
            return Ok(eventIntervals);
        }

        }


}
