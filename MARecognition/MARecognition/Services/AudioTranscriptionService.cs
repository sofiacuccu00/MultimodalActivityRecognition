using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Text;

namespace MARecognition.Services
{

    public class AudioTranscriptionService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;

        public AudioTranscriptionService()
        {
            var builder = Kernel.CreateBuilder();

            // Whisper model via Ollama
            builder.AddOllamaChatCompletion(
                modelId: "whisper-tiny:latest",
                endpoint: new Uri("http://localhost:11434")
            );

            _kernel = builder.Build();
            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<string> TranscribeAsync(string audioFilePath)
        {
            if (!File.Exists(audioFilePath))
                throw new FileNotFoundException($"Audio file not found: {audioFilePath}");

            var audioBytes = await File.ReadAllBytesAsync(audioFilePath);
            var audioBase64 = Convert.ToBase64String(audioBytes);

            var systemPrompt = """
                You are an automatic speech and sound transcription system.

                Transcribe the spoken content if present.
                If the audio contains non-verbal sounds, describe them briefly
                (e.g., impact, object falling, collision, shaking noise).

                Do NOT interpret or classify activities.
                Only describe what can be heard.
                """;

            var userPrompt = new StringBuilder();
            userPrompt.AppendLine("Here is the audio file:");
            userPrompt.AppendLine($"data:audio/wav;base64,{audioBase64}");

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemPrompt);
            chatHistory.AddUserMessage(userPrompt.ToString());

            var response = await _chatService.GetChatMessageContentAsync(
                chatHistory,
                kernel: _kernel
            );

            return response.ToString().Trim();
        }
    }
}
