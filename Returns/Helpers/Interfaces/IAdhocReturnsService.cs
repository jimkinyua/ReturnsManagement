using Returns.DTOs.Returns.Adhoc;
using Returns.Models;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers.Interfaces
{
    public interface IAdhocReturnsService
    {
        Task<IList<PendingAdHocReturnRequestDTO>> GetPendingAdHocReturnRequestsAsync(string? saccoId = null);
        Task<IList<PendingAdHocReturnRequestDTO>> GetRespondedAdHocReturnRequestsAsync(string? saccoId = null);
        Task<AdHocReturnRequest> CreateAdHocReturnRequestAsync(AdHocReturnRequestDTO dto, LoggedInEntity admin);
        Task<AdHocReturnRequest> CloseRequest(string RequestId, LoggedInEntity admin);
        Task<AdHocReturnRequest> RespondToAdHocReturnRequestAsync(AdHocReturnResponseDTO dto, LoggedInEntity loggedInEntity);
        Task<AdHocReturnRequestDetailsDTO> GetAdHocReturnRequestDetailsAsync(string requestId, LoggedInEntity admin);
        Task<IList<PendingAdHocReturnRequestDTO>> GetCompletedAdHocReturnRequestsAsync(string? saccoId = null);

    }
}
