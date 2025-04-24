namespace Returns.DTOs.WorkFlowTemplate
{
    public class PendingReturnDto
    {
        public string ReturnId { get; set; }
        public string SaccoId { get; set; }
        public string WorkFlowInstanceId { get; set; }
        public string CurrentRole { get; set; }
        public int CurrentStep { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime SubmittedDate { get; set; }
        public int? Rating { get; set; }
    }
}
