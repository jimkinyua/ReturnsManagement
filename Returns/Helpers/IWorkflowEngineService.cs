
using Returns.DTOs.WorkFlow_Engine;
using Returns.Models;

namespace Returns.Helpers
{
    public interface IWorkflowEngineService
    {
        Task<WorkflowStateDto> StartWorkflowAsync(Return SubmittedReturn, int Rating);
        Task<WorkflowStateDto> ApproveStepAsync(string workflowId, string userId, ApproveStepRequest request);
        Task<WorkflowStateDto> RejectStepAsync(string workflowId, string userId, RejectStepRequest request);
        Task<WorkflowStateDto> GetCurrentStateAsync(string returnId);
    }
}
