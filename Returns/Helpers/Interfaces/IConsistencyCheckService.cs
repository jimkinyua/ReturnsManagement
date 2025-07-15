using Returns.DTOs.Returns_Submission.DT;
using static Returns.Helpers.ReturnAnalysisHelper;

namespace Returns.Helpers.Interfaces
{
    public interface IConsistencyCheckService
    {
        Task<(bool IsValid, List<string> ProcessingSummary, List<ValidationError> ConsistencyErrors, bool HasConsistencyBeenChecked, List<object> FormData, string CommonPeriod)> CheckConsistencyAsync(NewReturnDTO createFormDTO, string ratingName, string SaccoType);
    }
}
