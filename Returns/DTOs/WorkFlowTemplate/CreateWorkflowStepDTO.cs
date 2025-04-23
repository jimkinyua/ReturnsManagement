namespace Returns.DTOs.WorkFlowTemplate
{
    public class CreateWorkflowStepDTO
    {
        public int Sequence { get; set; }
        public string RoleId { get; set; } = null!;
        public string WorkTemplateId { get; set; } = null!;
    }
}
