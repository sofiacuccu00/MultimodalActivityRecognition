using MARecognition.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using MARecognition.Models;

namespace MARecognition.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AudioController : ControllerBase
    {
        private readonly AudioTranscriptionService _audioTranscription;
        private readonly AudioActivityRecoService _audioRecognition;
        private readonly AudioPeakDetectService _peakDetect;

        public AudioController(
            AudioTranscriptionService audioTranscription,
            AudioActivityRecoService audioRecognition,
            AudioPeakDetectService peakDetect)
        {
            _audioTranscription = audioTranscription;
            _audioRecognition = audioRecognition;
            _peakDetect = peakDetect;
        }

        // Upload and trascription
        [HttpPost("transcribe")]
        public async Task<IActionResult> TranscribeAudio(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string folder = Path.Combine("audio");
            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, file.FileName);
            using (var stream = new FileStream(path, FileMode.Create))
                await file.CopyToAsync(stream);

            string transcription;
            try
            {
                transcription = await _audioTranscription.TranscribeAsync(path);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }

            return Ok(new { transcription, path });
        }

        // Activity analization
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

            string transcription;
            try
            {
                transcription = await _audioTranscription.TranscribeAsync(path);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }

            // Activity recognition by the trascription
            var audioItems = await _audioRecognition.RecognizeActivitiesAsync(transcription);

            return Ok(audioItems);
        }


        [HttpPost("peak")]
        public async Task<IActionResult> DetectPeak(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string folder = Path.Combine("audio");
            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, file.FileName);
            using (var stream = new FileStream(path, FileMode.Create))
                await file.CopyToAsync(stream);

            double peakTimestamp;
            try
            {
                peakTimestamp = _peakDetect.GetLoudestTimestamp(path);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }

            return Ok(new
            {
                peakTimestamp, // in seconds
                path
            });
        }
    }
}
