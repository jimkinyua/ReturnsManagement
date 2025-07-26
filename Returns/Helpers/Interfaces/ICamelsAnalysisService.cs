using Returns.DTOs.Returns_Analysis;

namespace Returns.Helpers.Interfaces
{
    public interface ICamelsAnalysisService
    {
        Task<CamelsRatingsDTO> CalculateCurrentDepositTakingAnalysisAsync(string groupId, string periodId, string saccoId);
        Task<CamelsRatingsDTO> CalculateAnalysisAsync(string groupId, string periodId, string saccoId, string saccoType);
    }
}
