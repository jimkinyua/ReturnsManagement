namespace Returns.DTOs.WorkFlowTemplate
{
    public class PendingReturnDto
    {
        public string ReturnId { get; set; } = string.Empty;
        public string SaccoName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public Boolean IsConsistent { get; set; }
        public string SaccoType { get; set; } = string.Empty;
        public string SaccoId { get; set; } = string.Empty;
        public string WorkFlowInstanceId { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public int CurrentStep { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime SubmittedDate { get; set; }
        public int? Rating { get; set; }
        public string Status { get; set; }  = string.Empty;
        public bool CanTakeAction { get; set; } 
    }
}
