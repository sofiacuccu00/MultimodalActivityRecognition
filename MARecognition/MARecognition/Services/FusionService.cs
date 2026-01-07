using System.Text;
using System.Text.Json;
using MARecognition.Miscellaneuous;
using MARecognition.Interfaces;
using MARecognition.Services.MultimodalActivityRecognition_CSD.Services;

namespace MARecognition.Services
{
    public class FusionService(SemanticKernelService sk) : IFusionService
    {
        private readonly SemanticKernelService _sk = sk;

        public async Task<FusionResult> FuseAsync(FusionRequest request)
        {
            var prompt = BuildPrompt(request);
            var response = await _sk.AskAsync(prompt);
            return ParseResponse(response);
        }

        private string BuildPrompt(FusionRequest req)
        {
            var sb = new StringBuilder();

            sb.AppendLine("You are an expert in multimodal human activity recognition.");
            sb.AppendLine("Given predictions from several sensors and contextual information,");
            sb.AppendLine("choose the SINGLE most likely action being performed.");
            sb.AppendLine();
            sb.AppendLine($"Context: {req.Context}");
            sb.AppendLine("Predictions:");

            if (req.Predictions != null)
            foreach (var kv in req.Predictions)
                sb.AppendLine($"- {kv.Key}: {kv.Value}");

            sb.AppendLine();
            sb.AppendLine("Respond ONLY in this JSON format:");
            sb.AppendLine("{ \"action\": \"...\", \"reasoning\": \"...\" }");

            return sb.ToString();
        }

        private FusionResult ParseResponse(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return new FusionResult
                {
                    FinalAction = root.GetProperty("action").GetString() ?? "unknown",
                    Reasoning = root.GetProperty("reasoning").GetString() ?? ""
                };
            }
            catch
            {
                return new FusionResult
                {
                    FinalAction = "unknown",
                    Reasoning = "LLM returned an unparsable response."
                };
            }
        }
    }
}
