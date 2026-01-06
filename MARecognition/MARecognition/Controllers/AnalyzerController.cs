using MARecognition.Models;
using MARecognition.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MARecognition.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyzerController : ControllerBase
    {
        private readonly VideoAudioFusionService _fusionService;

        public AnalyzerController(VideoAudioFusionService fusionService)
        {
            _fusionService = fusionService;
        }

        // analyze video and audio and give the multimodal log
        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze()
        {
            string videoPath = "videos/total.mp4";
            string audioPath = "audio/total.wav";
            string framesFolder = "data/frames";

            var result = await _fusionService.AnalyzeAsync(videoPath, audioPath, framesFolder);
            return Ok(result);
        }
    }

    [ApiController]
    [Route("api/download")]
    public class DownloadController : ControllerBase
    {
        [HttpGet("csv")]
        public IActionResult DownloadCsv()
        {
            string path = Path.Combine("output", "multimodal_log.csv");

            if (!System.IO.File.Exists(path))
                return NotFound("CSV non ancora generato");

            return File(
                System.IO.File.ReadAllBytes(path),
                "text/csv",
                "multimodal_log.csv"
            );
        }
    }


}
