using System.ComponentModel.DataAnnotations;

namespace Returns.DTOs.Returns.Admin
{
    public class TLWorkflowOverviewDTO
    {
        public string WorkflowInstanceId { get; set; } = string.Empty;
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public string SaccoId { get; set; } = string.Empty;
        public string SaccoName { get; set; } = string.Empty;
        public string PeriodId { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string CurrentAssigneeId { get; set; } = string.Empty;
        public string CurrentAssigneeName { get; set; } = string.Empty;
        public string WorkflowStatus { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public List<WorkflowCommentDTO> PreviousComments { get; set; } = new List<WorkflowCommentDTO>();
        public bool CanReassign { get; set; }
    }

    public class WorkflowCommentDTO
    {
        public string Comment { get; set; } = string.Empty;
        public string CommentedBy { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; }
        public string Action { get; set; } = string.Empty; // "Approved", "Rejected", "Commented", etc.
    }

    public class WorkflowReassignmentRequestDTO
    {
        [Required]
        public string WorkflowInstanceId { get; set; } = string.Empty;
        [Required]
        public string NewAssigneeId { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

    public class WorkflowReassignmentResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PreviousAssigneeId { get; set; } = string.Empty;
        public string NewAssigneeId { get; set; } = string.Empty;
        public DateTime ReassignedAt { get; set; }
    }
}

