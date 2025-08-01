
using Returns.DTOs.WorkFlow_Engine;
using Returns.DTOs.WorkFlowTemplate;
using Returns.Models;

namespace Returns.Helpers
{
    public interface IWorkflowEngineService
    {
        Task<WorkflowStateDto> StartWorkflowAsync(string periodId, string saccoId, string? returnSubmissionId);
        Task<WorkflowStateDto> ApproveStepAsync(ApproveStepRequestDTO approveStepRequestDTO, string userId);
        //Task<WorkflowStateDto> RejectStepAsync(string workflowId, string userId, RejectStepRequest request);
        Task<WorkflowStateDto> GetCurrentStateAsync(string? periodId, string? saccoId, string? returnSubmissionId);
        Task<List<PendingReturnDto>> GetPendingReturnsAsync(string userId);
        Task<List<CommentDetails>> GetComments(string periodId, string saccoId, string? returnSubmissionId);
        Task<WorkflowStateDto> RecommendForEnforcementAsync(RecommendStepRequest dto, string userId, string loggedInUserToken);
        Task<WorkflowStateDto> ReturnWithReservationsAsync(ReturnWithReservationsRequest dto, string userId);
    }
}
