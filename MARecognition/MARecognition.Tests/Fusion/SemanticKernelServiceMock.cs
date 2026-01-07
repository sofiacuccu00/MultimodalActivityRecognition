using System.Threading.Tasks;
using MARecognition.Services.MultimodalActivityRecognition_CSD.Services;

public class SemanticKernelServiceMock : SemanticKernelService
{
    private readonly string _response;

    public Func<string, Task<string>>? AskAsyncOverride { get; set; }

    public SemanticKernelServiceMock(string response)
    {
        _response = response;
    }

    public override Task<string> AskAsync(string question)
    {
        if (AskAsyncOverride != null)
            return AskAsyncOverride(question);

        return Task.FromResult(_response);
    }
}
