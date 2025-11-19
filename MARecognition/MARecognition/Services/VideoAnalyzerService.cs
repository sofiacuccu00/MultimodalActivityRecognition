using MARecognition.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Text;

namespace MARecognition.Services
{
    public class VideoAnalyzerService
    {
        private readonly Kernel _kernel;
        private readonly string _model;

        public VideoAnalyzerService(string modelName)
        {
            _model = modelName;

            var builder = Kernel.CreateBuilder();
            builder.AddOllamaChatCompletion(modelId: modelName, endpoint: new Uri("http://localhost:11434"));
            _kernel = builder.Build();
        }

        // Encode image in Base64
        private string EncodeImageToBase64(string imagePath)
        {
            var bytes = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(bytes);
        }

        // Main logic
        public async Task<List<EventLogItem>> RecognizeVideoActions(string frameDir, int numFrames)
        {
            if (numFrames < 3)
                return new List<EventLogItem>();

            //Gemma risulta limitata, per ora non abbiamo messo il prompt completo
            var systemPrompt = """
                You are an expert vision-language assistant for analyzing robot actions.
                You will be shown a sequence of three images...
                
                Respond ONLY with the action name.
                """;

            var results = new List<EventLogItem>();

            var chatService = _kernel.GetRequiredService<IChatCompletionService>();

            for (int i = 0; i < numFrames - 2; i++)
            {
                var framePaths = new[]
                {
                    Path.Combine(frameDir, $"frame_{i:00000}.jpg"),
                    Path.Combine(frameDir, $"frame_{i+1:00000}.jpg"),
                    Path.Combine(frameDir, $"frame_{i+2:00000}.jpg"),
                };

                // Crea il contesto dei frame precedenti
                string context;
                if (i == 0)
                    context = "No previous actions are known.";
                else if (i == 1)
                    context = $"The previous action for frame {i - 1:00000} was: '{results[i - 1].Activity}'.";
                else
                    context = $"The previous actions were:\n" +
                              $"- Frames {i - 2:00000}: '{results[i - 2].Activity}'\n" +
                              $"- Frames {i - 1:00000}: '{results[i - 1].Activity}'";

                // Prompt finale
                string userPrompt = $"{context}\nHere are three images of the robot (Frame 1 = oldest, Frame 3 = latest). What is the robot’s current action?";

                // Aggiungo i frame codificati in Base64 direttamente nel prompt
                foreach (var path in framePaths)
                {
                    if (!File.Exists(path))
                    {
                        Console.WriteLine($"Skipping frames {i} because {path} not found.");
                        continue;
                    }

                    var base64 = EncodeImageToBase64(path);
                    userPrompt += $"\nFrame {Path.GetFileName(path)}: data:image/jpeg;base64,{base64}";
                }

                // Crea la ChatHistory
                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage(systemPrompt);
                chatHistory.AddUserMessage(userPrompt);

                // Chiamata al modello
                var response = await chatService.GetChatMessageContentsAsync(chatHistory);

                string action = response[0].Content.Trim();

                Console.WriteLine($"Frames {i:00000}-{i + 2:00000} → {action}");

                results.Add(new EventLogItem(action, i));
            }

            return results;
        }
    }
}
