using Microsoft.AspNetCore.Mvc;
using MARecognition.Services;

namespace MARecognition.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AudioController : ControllerBase
    {
        private readonly AudioDropDetectionService _audioService;

        public AudioController(AudioDropDetectionService audioService)
        {
            _audioService = audioService;
        }

        //audio upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadAudio(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string folder = Path.Combine("audio");
            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, file.FileName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return Ok(new { message = "Audio uploaded", path });
        }

        // drop detection directly from the file
        [HttpPost("detect-drop")]
        public async Task<IActionResult> DetectDrop(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string folder = Path.Combine("audio");
            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, file.FileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // call the service for detection drop
            double dropTime = _audioService.DetectDropEvent(path);

            return Ok(new { dropTimeInSeconds = dropTime });
        }
    }
}
