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
        public string StepId { get; set; }
        public string Approver { get; set; }
        public string ApproverRole { get; set; }
        public string ApproverUserId { get; set; }
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

}   
