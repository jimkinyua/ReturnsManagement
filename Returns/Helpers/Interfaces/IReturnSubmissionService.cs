using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;

namespace Returns.Helpers.Interfaces
{
    public interface IReturnSubmissionService
    {
       async Task<IList<SubmissionResultDto>> UploadDraftAsync(NewReturnDTO dto);
    }
}
