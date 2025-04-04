using Returns.DTOs.Compliance;
using Returns.Models;

namespace Returns.Helpers.Interfaces
{
    public interface IReturnAssignmentService
    {
        Task<OperationResult> AssignReturnAsync(Return returnEntity, string saccoId);
        Task<OperationResult> ReassignReturnAsync(string returnId, string newUserId);
        Task<OperationResult> CheckSaccoAssignedUserAsync(string saccoId);
    }
}
