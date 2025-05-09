namespace Returns.DTOs.WorkFlow_Engine
{
    public class WorkflowStateDto
    {
        public string WorkflowId { get; set; }
        public string ReturnId { get; set; }
        public string CurrentStep { get; set; }
        public string Status { get; set; } // "Pending", "Approved", "Rejected"
        public int? Rating { get; set; }
        public List<WorkflowStepDto> NextSteps { get; set; } = new();
    }

    public class WorkflowStepDto
    {
        public string StepId { get; set; }
        public string Approver { get; set; }
        public string ApproverRole { get; set; }
        public string ApproverUserId { get; set; }
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
}
