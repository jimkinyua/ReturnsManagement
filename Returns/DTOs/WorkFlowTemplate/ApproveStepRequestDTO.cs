namespace Returns.DTOs.WorkFlowTemplate
{
    public class ApproveStepRequestDTO
    {
        public string WorkFlowInstanceId { get; set; } = null!;
        public string Comment { get; set; } = null!;
    }
}
