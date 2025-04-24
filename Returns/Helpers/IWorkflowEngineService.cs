
using Returns.DTOs.WorkFlow_Engine;
using Returns.DTOs.WorkFlowTemplate;
using Returns.Models;

namespace Returns.Helpers
{
    public interface IWorkflowEngineService
    {
        Task<WorkflowStateDto> StartWorkflowAsync(Return SubmittedReturn, int Rating);
        Task<WorkflowStateDto> ApproveStepAsync(string workflowInstanceId, string userId);
        Task<WorkflowStateDto> RejectStepAsync(string workflowId, string userId, RejectStepRequest request);
        Task<WorkflowStateDto> GetCurrentStateAsync(string returnId);
        Task<List<PendingReturnDto>> GetPendingReturnsAsync(string userId);
    }
}
