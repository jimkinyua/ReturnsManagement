using static Returns.Helpers.ReturnAnalysisHelper;

namespace Returns.DTOs.WorkFlowTemplate
{
    public class PendingReturnDto
    {
        public string WorkflowInstanceId { get; set; } = null!;
        public string PeriodId { get; set; } = null!;
        public string? ReturnSubmissionId { get; set; }
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public string SaccoType { get; set; } = null!;
        public string Period { get; set; } = null!;
        public DateTime SubmittedDate { get; set; }
        public bool IsConsistent { get; set; }
        public List<ValidationError> ConsistencyErrors { get; set; } = new();
        public int? Rating { get; set; }
        public string CurrentRole { get; set; } = null!;
        public string CurrentStep { get; set; }
        public string Status { get; set; } = null!;
        public string Type { get; set; } = null!;
        //public bool CanBeSeen { get; set; }
        public bool CanTakeAction { get; set; }
        public List<string> FormSubmissionIds { get; set; } = new();
    }
}
