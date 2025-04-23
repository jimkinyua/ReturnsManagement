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
    }
}
