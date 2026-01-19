namespace MARecognition.Interfaces
{
    public interface ILlmClient
    {
        Task<string> GetCompletionAsync(string prompt);
    }
}
