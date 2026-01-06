using MARecognition.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace MARecognition.Services
{
    
    public class AudioActivityRecoService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;

        public AudioActivityRecoService(string modelName = "karanchopda333/whisper")
        {
            
            var builder = Kernel.CreateBuilder();
            builder.AddOllamaChatCompletion(
                modelId: modelName,
                endpoint: new Uri("http://localhost:11434")
            );

            _kernel = builder.Build();
            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
        }

       
        public async Task<List<EventLogItem>> RecognizeActivitiesAsync(
            string transcription,
            int startTimeSeconds = 0,
            int? durationSeconds = null)
        {
            if (string.IsNullOrWhiteSpace(transcription))
                return new List<EventLogItem>();

            var systemPrompt = """
                You are an expert in robotic manipulation activity recognition.

                Given a textual description of sounds or audio transcription,
                classify robot activities into the following exact words:
                grasp, pick, hold, lower, shake, drop, idle

                Respond with a JSON array of objects:
                [
                  { "activity": "<activity>", "timestamp": <seconds> }
                ]

                Timestamp should be within the audio segment (approximate if necessary).

                Only include events actually heard in the audio.
                Do NOT invent actions.
                """;

            var userPrompt = $"Audio transcription: {transcription}";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemPrompt);
            chatHistory.AddUserMessage(userPrompt);

            var response = await _chatService.GetChatMessageContentAsync(chatHistory, kernel: _kernel);
            var resultText = response.ToString().Trim();

            // Convert JSON-like output into EventLogItem
            try
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<EventLogItem>>(resultText);
                return items ?? new List<EventLogItem>();
            }
            catch
            {
                
                return new List<EventLogItem>
                {
                    new EventLogItem("drop", startTimeSeconds)
                };
            }
        }
    }
}
