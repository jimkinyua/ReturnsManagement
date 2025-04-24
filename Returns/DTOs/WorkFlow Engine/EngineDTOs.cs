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
        public string RoleName { get; set; }
        public string AssignedUserId { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class ApproveStepRequest
    {
        public string Comment { get; set; }
    }

    public class RejectStepRequest
    {
        public string Reason { get; set; }
    }

    public class SubmitReturnRequest
    {
        public string SaccoId { get; set; }
    }
}
