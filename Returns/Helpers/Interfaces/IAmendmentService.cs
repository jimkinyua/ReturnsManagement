using Returns.DTOs.Returns;
using Returns.DTOs.Returns.Returns_Submission;
    using Returns.DTOs.Returns_Submission.DT;
    using Returns.Models;
    using static Returns.Helpers.TokenHelper;

    namespace Returns.Helpers.Interfaces
    {
        public interface IAmendmentService
        {
            public Task<AmendmentRequest> RespondToAdminAmendmentRequestAsync(SaccoAmendmentResponseDTO dto, LoggedInEntity loggedInEntity);
            public Task<IList<SubmissionResultDto>> DoAmendmentIfNecessasy(NewReturnDTO dto, LoggedInEntity sacco);
            public Task ReviewAmendmentRequest(string requestId, bool approve, string reviewerId);
            public Task<AmendmentRequest> CreateAmendRequestForSacco(AmendmentRequestDTO dto, LoggedInEntity loggedInEntity);
            public Task<IList<PendingAmendmentRequestDTO>> GetPendingAmendmentRequestsAsync();
            public Task<AmendmentRequestDetailsDTO> GetAmendmentRequestDetailsAsync(string requestId, LoggedInEntity admin);
            public Task<AmendmentRequest> CreateAdminAmendmentRequestAsync(AdminAmendmentRequestDTO dto, LoggedInEntity admin);

    }
}
