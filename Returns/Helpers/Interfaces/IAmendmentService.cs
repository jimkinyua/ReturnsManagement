using Returns.DTOs.Returns;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Models;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers.Interfaces
{
    public interface IAmendmentService
    {
        Task<ReturnSubmission> GetSubmissionUsingReturnIdAsync(string submissionId, string saccoId);
        Task<ReturnSubmission> GetSubmissionUsingExpectedIdAsync(string submissionId, string saccoId, Boolean Filing = false);
        Task<AmendmentRequest> CreateAmendRequestForSacco(AmendmentRequestDTO dto, LoggedInEntity loggedInEntity);
        Task<AmendmentRequest> RespondToAdminAmendmentRequestAsync(SaccoAmendmentResponseDTO dto, LoggedInEntity loggedInEntity);
        Task<AmendmentRequest> CreateAdminAmendmentRequestAsync(AdminAmendmentRequestDTO dto, LoggedInEntity admin);
        Task<IList<PendingAmendmentRequestDTO>> GetAmendmentRequestsPendingAdminApprovalAsync();
        Task<IList<PendingAmendmentRequestDTO>> GetAmendmentRequestsPendingSaccoResponseAsync();
        Task<IList<PendingAmendmentRequestDTO>> GetAdminInitiatedAmendmentRequestsAsync();
        Task<AmendmentRequestDetailsDTO> GetAmendmentRequestDetailsAsync(string requestId, LoggedInEntity admin);
        Task ReviewAmendmentRequest(string requestId, bool approve, string reviewerId);

    }
}
