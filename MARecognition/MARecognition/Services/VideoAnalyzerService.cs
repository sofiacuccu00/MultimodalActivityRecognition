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

            //Prompt for llava
            var systemPrompt = """
                You are a vision-language assistant specialized in recognizing robot manipulation actions.

                You will be shown a sequence of three images of a robot interacting with an object.
                Your task is to classify the robot’s current action (the action in the third image) based ONLY on what you see in the images.

                Use only visible evidence: object position, gripper contact, and motion direction.
                Do NOT guess intentions or use shadows or lighting to infer motion.

                Valid actions (respond exactly with one of these words):
                grasp, pick, hold, lower, shake, drop, idle

                Action definitions:
                - grasp: robot is attempting to acquire the object without lifting it (approach, align gripper, close gripper). Object must remain on the surface.
                - pick: robot lifts the object vertically. Object must rise clearly, no lateral movement, gripper fully closed.
                - hold: robot is holding object mid-air without movement. Object suspended, gripper closed.
                - lower: robot moves a held object downward without releasing it.
                - shake: robot moves or rotates a held object while gripping it. Do not classify as shake if motion is only upward (pick) or no motion (hold).
                - drop: robot releases the object. Gripper open or not in contact, object falling or resting independently.
                - idle: no significant action detected.

                Rules:
                - Respond with exactly ONE word from the valid actions.
                - Do NOT explain or add extra text.
                - Use only the visual evidence in the three frames.
                
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

                // context from recent frames
                string context;
                if (i == 0)
                    context = "No previous actions are known.";
                else if (i == 1)
                    context = $"The previous action for frame {i - 1:00000} was: '{results[i - 1].Activity}'.";
                else
                    context = $"The previous actions were:\n" +
                              $"- Frames {i - 2:00000}: '{results[i - 2].Activity}'\n" +
                              $"- Frames {i - 1:00000}: '{results[i - 1].Activity}'";

                // final prompt
                string userPrompt = $"{context}\nHere are three images of the robot (Frame 1 = oldest, Frame 3 = latest). What is the robot’s current action?";

                // add frames into the context
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

                // chathistory creation
                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage(systemPrompt);
                chatHistory.AddUserMessage(userPrompt);

                //call the model
                var response = await chatService.GetChatMessageContentsAsync(chatHistory);

                string action = response[0].Content.Trim();

                Console.WriteLine($"Frames {i:00000}-{i + 2:00000} → {action}");

                results.Add(new EventLogItem(action, i));
            }

            //TODO accorpare le azioni simili

            return results;
        }
    }
}
