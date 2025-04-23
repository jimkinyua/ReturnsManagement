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
}
