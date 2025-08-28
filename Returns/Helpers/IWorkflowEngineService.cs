
using Returns.DTOs.WorkFlow_Engine;
using Returns.DTOs.WorkFlowTemplate;
using Returns.DTOs.Returns.Admin;
using Returns.Models;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers
{
    public interface IWorkflowEngineService
    {
        Task<WorkflowStateDto> StartWorkflowAsync(string periodId, string saccoId, string? returnSubmissionId);
        Task<WorkflowStateDto> ApproveStepAsync(ApproveStepRequestDTO approveStepRequestDTO, string userId);
        //Task<WorkflowStateDto> RejectStepAsync(RejectStepRequest request, string userId);
        Task<WorkflowStateDto?> GetCurrentStateAsync(string? periodId, string? saccoId, string? returnSubmissionId);
        Task<List<PendingReturnDto>> GetPendingReturnsAsync(string userId);
        Task<List<CommentDetails>> GetComments(string periodId, string saccoId, string? returnSubmissionId);
        Task<WorkflowStateDto> RecommendForEnforcementAsync(RecommendStepRequest dto, string userId, string loggedInUserToken);
        Task<WorkflowStateDto> ReturnWithReservationsAsync(ReturnWithReservationsRequest dto, string userId);
        Task<List<TLWorkflowOverviewDTO>> GetTLWorkflowOverviewAsync(string teamLeadId);
        Task<WorkflowReassignmentResultDTO> ReassignWorkflowAsync(WorkflowReassignmentRequestDTO request, LoggedInEntity loggedInEntity);
        Task<List<RatedSaccoForInspectionDto>> GetRatedSaccosForInspectionAsync();
        Task<InspectionRecommendationResult> RecommendForInspectionAsync(RecommendForInspectionRequest request, string userId, string loggedInUserToken);
    }
}
