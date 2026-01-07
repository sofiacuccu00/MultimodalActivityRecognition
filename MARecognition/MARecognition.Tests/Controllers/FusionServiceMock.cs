using MARecognition.Interfaces;
using MARecognition.Miscellaneuous;
using MARecognition.Services;

public class FusionServiceMock : IFusionService
{
    public FusionRequest? CapturedRequest { get; private set; }
    private readonly FusionResult _response;

    public FusionServiceMock(FusionResult response)
    {
        _response = response;
    }

    public Task<FusionResult> FuseAsync(FusionRequest request)
    {
        CapturedRequest = request;
        return Task.FromResult(_response);
    }
}
