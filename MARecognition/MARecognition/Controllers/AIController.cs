using MARecognition.Services.MultimodalActivityRecognition_CSD.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MARecognition.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly SemanticKernelService _service;

        public AIController(SemanticKernelService service)
        {
            _service = service;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] QuestionDto req)
        {
            var result = await _service.AskAsync(req.Question);
            return Ok(new { answer = result });
        }
    }

    public class QuestionDto { public string Question { get; set; } }


}
