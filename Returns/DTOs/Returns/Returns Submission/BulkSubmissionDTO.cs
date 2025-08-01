using Returns.DTOs.Returns.Returns_Submission;

namespace Returns.DTOs.Returns.Returns_Submission
{
    public class BulkSubmissionRequestDTO
    {
        public string PeriodId { get; set; } = null!;
        public string? SaccoTypeId { get; set; }
    }

    public class BulkSubmissionResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int TotalForms { get; set; }
        public int SuccessfullySubmitted { get; set; }
        public int FailedSubmissions { get; set; }
        public bool IsPeriodComplete { get; set; } 
        public int MissingCount { get; set; }
        public string SubmissionId { get; set; } = "";
        public List<BulkSubmissionDetailDTO> Details { get; set; } = new List<BulkSubmissionDetailDTO>();
    }

    public class BulkSubmissionDetailDTO
    {
        public string ExpectedReturnId { get; set; } = null!;
        public string FormName { get; set; } = null!;
        public string FormCode { get; set; } = null!;
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? SubmissionId { get; set; }
        public SubmissionStatus Status { get; set; }
    }
}