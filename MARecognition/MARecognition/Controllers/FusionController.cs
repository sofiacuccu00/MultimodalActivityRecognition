using Microsoft.AspNetCore.Mvc;
using MARecognition.Miscellaneuous;
using MARecognition.Interfaces;

namespace MARecognition.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FusionController(IFusionService fusionService) : ControllerBase
    {
        private readonly IFusionService _fusionService = fusionService;

        [HttpPost("fuse")]
        public async Task<ActionResult<FusionResult>> Fuse([FromBody] FusionRequest request)
        {
            if (request.Predictions == null || request.Predictions.Count == 0)
                return BadRequest("No predictions provided.");

            if (string.IsNullOrWhiteSpace(request.Context))
                return BadRequest("Context is required.");

            var result = await _fusionService.FuseAsync(request);
            return Ok(result);
        }

    }
}
