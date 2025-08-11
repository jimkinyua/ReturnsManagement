using Returns.Models;
using static Returns.DTOs.WorkFlowTemplate.WorkflowStepDTO;

namespace Returns.DTOs.WorkFlowTemplate
{
    public class WorkflowTemplateDTO
    {
        public string TemplateId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public List<WorkflowStepDTO> Steps { get; set; } = new List<WorkflowStepDTO>();
    }
    public class StepAssigneeDTO
    {
        public string StepId { get; set; } = null!;
        public StepAssignee AssigneeType { get; set; }
        public bool IsResolved { get; set; }
        public string? Message { get; set; }
        public List<AssigneeDTO> Assignees { get; set; } = new();
    }
}
