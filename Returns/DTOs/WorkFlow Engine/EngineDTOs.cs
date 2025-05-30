using System.ComponentModel.DataAnnotations;

namespace Returns.DTOs.WorkFlow_Engine
{
    public class WorkflowStateDto
    {
        public string WorkflowInstanceId { get; set; }
        public string ReturnId { get; set; }
        public string CurrentStepId { get; set; }
        public string CurrentApproverId { get; set; }
        public string Status { get; set; } // "Pending", "RecommendForApproval", "Rejected"
        public int? Rating { get; set; }
        public Boolean IsFirst { get; set; } = false;
        public Boolean IsLast { get; set; } = false;
        public List<WorkflowStepDto> NextSteps { get; set; } = new();
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
