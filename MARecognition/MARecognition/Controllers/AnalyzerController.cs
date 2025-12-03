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

        /// <summary>
        /// Analizza video e audio e restituisce log multimodale
        /// </summary>
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
}
