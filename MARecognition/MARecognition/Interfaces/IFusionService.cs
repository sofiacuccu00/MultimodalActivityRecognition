using MARecognition.Miscellaneuous;

namespace MARecognition.Interfaces
{
    public interface IFusionService
    {
        Task<FusionResult> FuseAsync(FusionRequest request);
    }
}
