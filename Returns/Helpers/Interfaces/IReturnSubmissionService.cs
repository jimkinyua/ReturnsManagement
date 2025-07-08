using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;

namespace Returns.Helpers.Interfaces
{
    public interface IReturnSubmissionService
    {
        Task<IList<SubmissionResultDto>> UploadDraftAsync(NewReturnDTO dto, string SaccoType, string SaccoId);
        Task<IList<SubmissionResultDto>> SubmitFinalAsync(string submissionId);
    }
}
