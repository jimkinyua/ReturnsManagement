using System.ComponentModel.DataAnnotations;
using static Returns.Helpers.ReturnAnalysisHelper;

namespace Returns.DTOs.WorkFlow_Engine
{
    public class WorkflowStateDto
    {
        public string WorkflowInstanceId { get; set; } = null!;
        public string PeriodId { get; set; } = null!;
        public string? ReturnSubmissionId { get; set; }
        public string SaccoId { get; set; } = null!;
        public string CurrentStepId { get; set; } = null!;
        public string CurrentApproverId { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int? Rating { get; set; }
        //public string Type { get; set; } = "Standalone"; // "QGroup" or "Standalone"
        //public bool CanBeSeen { get; set; } = true;
        public bool IsFirst { get; set; } = false;
        public bool IsLast { get; set; } = false;
        public bool IsConsistent { get; set; } = true;
        public List<WorkflowStepDto> NextSteps { get; set; } = new();
        public List<ValidationError> ConsistencyErrors { get; set; } = new();
    }


    public class WorkflowStepDto
    {
        public string StepId { get; set; } = string.Empty;
        public string Approver { get; set; } = string.Empty;
        public string ApproverRole { get; set; } = string.Empty;
        public string ApproverUserId { get; set; } = string.Empty;
        public Boolean IsFirst { get; set; } = false;
        public Boolean IsLast { get; set; } = false;
    }


    public class CommentDetails
    {
        public string Comment { get; set; }
        public string UserId { get; set; }
        public string ApproverName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ApproveStepRequest
    {
        public string Comment { get; set; }
    }

    public class RejectStepRequest
    {
        public string Reason { get; set; }
        public string WorkFlowInstanceId { get; set; } = null!;

    }

    public class SubmitReturnRequest
    {
        public string SaccoId { get; set; }
    }

    public sealed record RecommendStepRequest
    {
        [Required] public string WorkFlowInstanceId { get; set; } = string.Empty;
        [Required] public string Reason { get; init; } = string.Empty;
        public string? Classification { get; init; }
    }

    public sealed record ReturnWithReservationsRequest
    {
        [Required] public string WorkFlowInstanceId { get; set; } = string.Empty;
        [Required] public string Comment { get; set; } = string.Empty;
    }

    public sealed record RecommendForInspectionRequest
    {
        [Required] public string WorkFlowInstanceId { get; set; } = string.Empty;
        [Required] public string Reason { get; init; } = string.Empty;
        public string? Classification { get; init; }
        public string? RiskLevel { get; init; }
        public string? ComplaintsCount { get; init; }
    }

    public sealed record InspectionModuleRequest
    {
        public string reference { get; set; } = string.Empty;
        public string saccoId { get; set; } = string.Empty;
        public string saccoName { get; set; } = string.Empty;
        public string rating { get; set; } = string.Empty;
        public string tier { get; set; } = string.Empty;
        public string reason { get; set; } = string.Empty;
        public string source { get; set; } = string.Empty;
    }

    public sealed record InspectionRecommendationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? InspectionCaseId { get; set; }
        public DateTime RecommendedAt { get; set; }
    }

    public sealed record RatedSaccoForInspectionDto
    {
        public string WorkflowInstanceId { get; set; } = string.Empty;
        public string SaccoId { get; set; } = string.Empty;
        public string SaccoName { get; set; } = string.Empty;
        public string PeriodId { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime RatedAt { get; set; }
        public DateTime WorkflowCompletedAt { get; set; }
        public bool CanRecommendForInspection { get; set; } = true;
    }
}
