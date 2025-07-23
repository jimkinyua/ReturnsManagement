using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers.Interfaces
{
    public interface IAmendmentService
    {
        public Task<IList<SubmissionResultDto>> DoAmendmentIfNecessasy(NewReturnDTO dto, LoggedInEntity sacco);
        public Task ReviewAmendmentRequest(string requestId, bool approve, string reviewerId);
        public Task CreateAmendRequestForSacco(string submissionId, string officerId, string reason);


    }
}
