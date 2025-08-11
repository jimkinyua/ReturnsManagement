using Returns.Models;

namespace Returns.DTOs.WorkFlowTemplate
{
    public class CreateWorkflowStepDTO
    {
        public int Sequence { get; set; }
        public string RoleId { get; set; } = null!;
        public string WorkTemplateId { get; set; } = null!;
        public StepAssignee AssigneeType { get; set; }    
        public string? SpecificUserId { get; set; }
    }
}
