using Returns.Models;

namespace Returns.DTOs.WorkFlowTemplate
{
    public class WorkflowStepDTO
    {
        public string StepId { get; set; } = null!;
        public int Sequence { get; set; }
        public string RoleId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public string TemplateId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string AssigneeType { get; set; } = string.Empty;
        public string? SpecificUserId { get; set; }
        public bool AssigneeResolved { get; set; }
        public string? AssigneeMessage { get; set; }
        public AssigneeDTO? Assignee { get; set; }   // single user for user-specific model
        public class AssigneeDTO
        {
            public string UserId { get; set; } = null!;
            public string FullName { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string? RoleName { get; set; }
        }
    }
}
