namespace Returns.DTOs.Returns
{
    public class SubmittedReturnSummaryDto
    {
        public string ReturnId { get; set; } = "";
        public string PeriodId { get; set; } = "";
        public string PeriodName { get; set; } = "";
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string Year { get; set; } = "";
        public DateTime SubmittedAt { get; set; }
        public string SaccoId { get; set; } = "";
        public string SaccoName { get; set; } = "";
        public string SaccoType { get; set; } = "";
        
        // Submission status
        public bool IsConsistent { get; set; }
        public string? ConsistencyErrors { get; set; }
        public int TotalFormsSubmitted { get; set; }
        public int TotalFormsExpected { get; set; }
        public int LateSubmissions { get; set; }
        
        // Version tracking
        public int VersionNumber { get; set; }
        public bool IsActiveVersion { get; set; }
        public string? PreviousVersionId { get; set; }
        
        // Approval status
        public string ApprovalStatus { get; set; } = "";
        public DateTime? ApprovedDate { get; set; }
        
        // Forms detail
        public List<SubmittedFormDto> SubmittedForms { get; set; } = new();
    }
    
    public class SubmittedFormDto
    {
        public string FormId { get; set; } = "";
        public string FormCode { get; set; } = "";
        public string FormName { get; set; } = "";
        public DateTime? SubmittedDate { get; set; }
        public bool WasLate { get; set; }
        public int DaysLate { get; set; }
        public bool RequiresResubmission { get; set; }
    }
}