using MARecognition.Services;
using Microsoft.AspNetCore.Mvc;

namespace MARecognition.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AudioController : ControllerBase
    {
        private readonly AudioTranscriptionService _audioTranscription;
        private readonly AudioActivityRecoService _audioRecognition;

        public AudioController(
            AudioTranscriptionService audioTranscription,
            AudioActivityRecoService audioRecognition)
        {
            _audioTranscription = audioTranscription;
            _audioRecognition = audioRecognition;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAudio(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string folder = Path.Combine("audio");
            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, file.FileName);
            using (var stream = new FileStream(path, FileMode.Create))
                await file.CopyToAsync(stream);

            return Ok(new { message = "Audio uploaded", path });
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeAudio(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string folder = Path.Combine("audio");
            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, file.FileName);
            using (var stream = new FileStream(path, FileMode.Create))
                await file.CopyToAsync(stream);

            
            string transcription = await _audioTranscription.TranscribeAsync(path);
            var audioItems = await _audioRecognition.RecognizeActivitiesAsync(transcription);

            return Ok(audioItems);
        }
    }
}
