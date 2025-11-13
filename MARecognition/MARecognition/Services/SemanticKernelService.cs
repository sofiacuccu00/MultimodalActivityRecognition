namespace MARecognition.Services
{
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.ChatCompletion;
    using Microsoft.SemanticKernel.Connectors.Ollama;

    namespace MultimodalActivityRecognition_CSD.Services
    {
        public class SemanticKernelService
        {
            private readonly Kernel _kernel;
            private readonly IChatCompletionService _chatService;

            public SemanticKernelService()
            {
                // 1️⃣ Crea Kernel
                var builder = Kernel.CreateBuilder();

                // 2️⃣ Aggiungi Ollama + modello Gemma
                builder.AddOllamaChatCompletion(
                    modelId: "gemma3:1b",
                    endpoint: new Uri("http://localhost:11434")
                );

                _kernel = builder.Build();

                // 3️⃣ Ottieni Chat Service
                _chatService = _kernel.GetRequiredService<IChatCompletionService>();
            }

            public async Task<string> AskAsync(string question)
            {
                var chatHistory = new ChatHistory();
                chatHistory.AddUserMessage(question);

                var result = await _chatService.GetChatMessageContentAsync(
                    chatHistory,
                    kernel: _kernel
                );

                return result.ToString();
            }
        }
    }


}
